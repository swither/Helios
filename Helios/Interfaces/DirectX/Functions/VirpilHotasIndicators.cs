// Copyright 2023 Helios Contributors
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

using CommandLine.Text;
using GadrocsWorkshop.Helios.Interfaces.DirectX;
using HidSharp;
using NLog;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GadrocsWorkshop.Helios.Interfaces.Vendor.Functions
{
    public class VirpilHotasIndicators : IHotasFunctions
    {

        private string _deviceName;
        private Joystick _device;
        private HidDevice _hotasDevice;
        private HidStream _hotasStream;

        private HeliosValue _indicatorValue;
        private DirectXControllerInterface _sourceInterface;
        private byte[][] _writeBuffers = new byte[][] { new byte[38], new byte[38], new byte[38], new byte[38] };

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private enum VirpilSubDeviceFlagEnum
        {
            DEFAULT = 0x0064,
            ADDONGRIPS = 0x0065,
            ONBOARDLEDS = 0x0066,
            SLAVELEDS = 0x0067
        }

        public VirpilHotasIndicators(DirectXControllerInterface sourceInterface, Joystick joystickDevice, string deviceName, string name)
        {
            _deviceName = deviceName;
            _device = joystickDevice;
            _sourceInterface = sourceInterface;
        }

        public void CreateActionsAndValues()
        {
            DoBuild();
        }

        private void DoBuild()
        {
            foreach (VirpilSubDeviceFlagEnum subDevice in Enum.GetValues(typeof(VirpilSubDeviceFlagEnum)))
            {
                int j = 0;
                _writeBuffers[(int)subDevice - 0x64][j++] = (byte)0x02;
                _writeBuffers[(int)subDevice - 0x64][j++] = (byte)subDevice;
                _writeBuffers[(int)subDevice - 0x64][j++] = (byte)0x25;  // length?
                _writeBuffers[(int)subDevice - 0x64][j++] = (byte)0x00;  // address?
                _writeBuffers[(int)subDevice - 0x64][j++] = (byte)0x00;  // address?

                for (int i = 0; i < 32; i++)
                {
                    _writeBuffers[(int)subDevice - 0x64][i + j] = (byte)0x00; // start without changing any of the indicators.
                    _indicatorValue = new HeliosValue(_sourceInterface, new BindingValue(null), $"{subDevice} {_deviceName}", $"indicator {i + 1}", $"the color value of indicator {i + 1} on {subDevice}.", "Number representing the binary representation the indicator's color 0b??GGRRBB", BindingValueUnits.Numeric);
                    _indicatorValue.Execute += new HeliosActionHandler(Action_Execute);
                    _sourceInterface.Values.Add(_indicatorValue);
                    _sourceInterface.Actions.Add(_indicatorValue);
                }
                _writeBuffers[(int)subDevice - 0x64][37] = (byte)0xf0;   // marker for the  end of the buffer
            }
        }

        void Action_Execute(object action, HeliosActionEventArgs e)
        {
            if (action is HeliosValue hAction)
            {
                if (Int32.TryParse(hAction.Name.Split(' ')[1], out Int32 indicatorIndex))
                {
                    if (Enum.TryParse(hAction.Device.Split(' ')[0], out VirpilSubDeviceFlagEnum subDevice))
                    {
                        _writeBuffers[(int)subDevice - 0x64][indicatorIndex - 1 + 5] = (byte)(0x80 | (e.Value.DoubleValue > 255 ? (byte)0xff : e.Value.DoubleValue < 0 ? (byte)0x00 : (byte)Convert.ToInt16(e.Value.DoubleValue))); // set the changed flag
                        SendHidData(_writeBuffers[(int)subDevice - 0x64]);
                        _writeBuffers[(int)subDevice - 0x64][indicatorIndex - 1 + 5] &= (byte)0x7F;  //Reset the changed flag
                    }
                }
            }
        }

        public void OpenHidDevice() { OpenHidDevice(_device); }
        public void OpenHidDevice(Joystick device)
        {
            _hotasDevice = DeviceList.Local.GetHidDevices().Where(d => d.VendorID == device.Properties.VendorId && d.ProductID == device.Properties.ProductId && d.ProductName == device.Properties.ProductName).FirstOrDefault(d => d.GetMaxFeatureReportLength() > 0);
            if (_hotasDevice == null)
            {
                Logger.Info($"Unable to find USB device with VendorID: {device.Properties.VendorId} and ProductID: {device.Properties.ProductId}.");
            }
        }

        public void SendHidData(byte[] buffer)
        {
            if (_hotasDevice != null)
            {
                if (_hotasDevice.TryOpen(out _hotasStream))
                {
                    try
                    {
                        if (_hotasStream.CanWrite)
                        {
                            _hotasStream.SetFeature(buffer, 0, buffer.Length);
                            _hotasStream.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error while writing to {_hotasDevice.VendorID} {_hotasDevice.GetProductName()} {ex.Message}.");
                        if (_hotasStream != null)
                        {
                            _hotasStream.Close();
                        }
                        _hotasDevice = null;
                    }
                }
            }
        }

        public void Reset()
        {
            foreach (VirpilSubDeviceFlagEnum subDevice in Enum.GetValues(typeof(VirpilSubDeviceFlagEnum)))
            {
                int j = 0;
                _writeBuffers[(int)subDevice - 0x64][j++] = (byte)0x02;
                _writeBuffers[(int)subDevice - 0x64][j++] = (byte)subDevice;
                _writeBuffers[(int)subDevice - 0x64][j++] = (byte)0x25;  // length?
                _writeBuffers[(int)subDevice - 0x64][j++] = (byte)0x00;  // address?
                _writeBuffers[(int)subDevice - 0x64][j++] = (byte)0x00;  // address?

                for (int i = 0; i < 32; i++)
                {
                    _writeBuffers[(int)subDevice - 0x64][i + j] = (byte)0x00; // start without changing any of the indicators.

                }
                _writeBuffers[(int)subDevice - 0x64][37] = (byte)0xf0;   // marker for the  end of the buffer
            }
        }
    }
}

