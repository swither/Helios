//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace GadrocsWorkshop.Helios
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// TemplateManager gives access to saving and loading control templates.
    /// </summary>
    public class TemplateManager : ITemplateManager
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private HeliosTemplateCollection _userTemplates = new HeliosTemplateCollection();
        private List<HeliosTemplate> _moduleTemplates = new List<HeliosTemplate>();

        private string _userTemplateDirectory = "";

        internal TemplateManager(string userTemplateDirectory, string userPanelTemplateDirectory)
        {
            Logger.Debug($"Helios will load user templates from {Util.Anonymizer.Anonymize(userTemplateDirectory)}");
            Logger.Debug($"Helios will load user panel templates from {Util.Anonymizer.Anonymize(userPanelTemplateDirectory)}");

            _userTemplateDirectory = userTemplateDirectory;

            PopulateUserTemplatesCollection();

            _userTemplates.CollectionChanged += new NotifyCollectionChangedEventHandler(UserTemplates_CollectionChanged);
        }

        #region Properties

        public HeliosTemplateCollection UserTemplates
        {
            get
            {
                return _userTemplates;
            }
        }

        public IList<HeliosTemplate> ModuleTemplates
        {
            get
            {
                return _moduleTemplates.AsReadOnly();
            }
        }

        #endregion

        internal void LoadModuleTemplates(string moduleName)
        {
            string templatesDirectory = Path.Combine(ConfigManager.ApplicationPath, "Templates", moduleName);
            if (Directory.Exists(templatesDirectory))
            {
                Logger.Debug($"Loading module templates for module '{moduleName}' from '{Util.Anonymizer.Anonymize(templatesDirectory)}'");
                LoadTemplateDirectory(_moduleTemplates, templatesDirectory, false);
            }
            string pluingTemplatesDirectory = Path.Combine(ConfigManager.ApplicationPath, "Plugins", "Templates", moduleName);
            if (Directory.Exists(pluingTemplatesDirectory))
            {
                Logger.Debug($"Loading module templates for plugin '{moduleName}' from '{pluingTemplatesDirectory}'");
                LoadTemplateDirectory(_moduleTemplates, pluingTemplatesDirectory, false);
            }
        }

        private void PopulateUserTemplatesCollection()
        {
            Logger.Debug("Loading user templates from documents folder");
            LoadTemplateDirectory(_userTemplates, _userTemplateDirectory, true);
        }

        private void LoadTemplateDirectory(IList<HeliosTemplate> templates, string directory, bool userTemplates)
        {
            foreach (string templateFile in Directory.GetFiles(directory, "*.htpl"))
            {
                HeliosTemplate template = LoadTemplate(templateFile, userTemplates);

                // prevent crash on duplicate key
                if (templates is HeliosTemplateCollection indexed)
                {
                    string key = indexed.GetKeyForItem(template);
                    if (indexed.ContainsKey(key))
                    {
                        Logger.Error($"ignored duplicate template '{Util.Anonymizer.Anonymize(templateFile)}' already loaded from another location");
                        continue;
                    }
                }
                templates.Add(template);
            }

            foreach (string subDirectory in Directory.GetDirectories(directory))
            {
                LoadTemplateDirectory(templates, subDirectory, userTemplates);
            }
        }

        private HeliosTemplate LoadTemplate(string path, bool isUserTemplate)
        {
            HeliosTemplate template = new HeliosTemplate(isUserTemplate);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.CloseInput = true;

            XmlReader reader = XmlReader.Create(path, settings);
            template.ReadXml(reader);
            reader.Close();

            return template;
        }


        private string GetTemplatePath(HeliosTemplate template)
        {
            return Path.Combine(_userTemplateDirectory, template.Category, template.Name + ".htpl");
        }

        void UserTemplates_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (HeliosTemplate template in e.OldItems)
                {
                    if (template != null)
                    {
                        string path = GetTemplatePath(template);
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (HeliosTemplate template in e.NewItems)
                {
                    if (template != null)
                    {
                        string path = GetTemplatePath(template);
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }

                        string directory = Path.GetDirectoryName(path);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        XmlWriter writer = new XmlTextWriter(path, Encoding.UTF8);
                        template.WriteXml(writer);
                        writer.Close();
                    }
                }
            }
        }
    }
}
