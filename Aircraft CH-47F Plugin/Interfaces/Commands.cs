﻿//  Copyright 2024 Helios Contributors
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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.CH47F
{
    internal class Commands
    {
        internal enum Button
        {
            Button_1 = 3001,
            Button_2 = 3002,
            Button_3 = 3003,
            Button_4 = 3004,
            Button_5 = 3005,
            Button_6 = 3006,
            Button_7 = 3007,
            Button_8 = 3008,
            Button_9 = 3009,
            Button_10 = 3010,
            Button_11 = 3011,
            Button_12 = 3012,
            Button_13 = 3013,
            Button_14 = 3014,
            Button_15 = 3015,
            Button_16 = 3016,
            Button_17 = 3017,
            Button_18 = 3018,
            Button_19 = 3019,
            Button_20 = 3020,
            Button_21 = 3021,
            Button_22 = 3022,
            Button_23 = 3023,
            Button_24 = 3024,
            Button_25 = 3025,
            Button_26 = 3026,
            Button_27 = 3027,
            Button_28 = 3028,
            Button_29 = 3029,
            Button_30 = 3030,
            Button_31 = 3031,
            Button_32 = 3032,
            Button_33 = 3033,
            Button_34 = 3034,
            Button_35 = 3035,
            Button_36 = 3036,
            Button_37 = 3037,
            Button_38 = 3038,
            Button_39 = 3039,
            Button_40 = 3040,
            Button_41 = 3041,
            Button_42 = 3042,
            Button_43 = 3043,
            Button_44 = 3044,
            Button_45 = 3045,
            Button_46 = 3046,
            Button_47 = 3047,
            Button_48 = 3048,
            Button_49 = 3049,
            Button_50 = 3050,
            Button_51 = 3051,
            Button_52 = 3052,
            Button_53 = 3053,
            Button_54 = 3054,
            Button_55 = 3055,
            Button_56 = 3056,
            Button_57 = 3057,
            Button_58 = 3058,
            Button_59 = 3059,
            Button_60 = 3060,
            Button_61 = 3061,
            Button_62 = 3062,
            Button_63 = 3063,
            Button_64 = 3064,
            Button_65 = 3065,
            Button_66 = 3066,
            Button_67 = 3067,
            Button_68 = 3068,
            Button_69 = 3069,
            Button_70 = 3070,
            Button_71 = 3071,
            Button_72 = 3072,
            Button_73 = 3073,
            Button_74 = 3074,
            Button_75 = 3075,
            Button_76 = 3076,
            Button_77 = 3077,
            Button_78 = 3078,
            Button_79 = 3079,
            Button_80 = 3080,
            Button_81 = 3081,
            Button_82 = 3082,
            Button_83 = 3083,
            Button_84 = 3084,
            Button_85 = 3085,
            Button_86 = 3086,
            Button_87 = 3087,
            Button_88 = 3088,
            Button_89 = 3089,
            Button_90 = 3090,
            Button_91 = 3091,
            Button_92 = 3092,
            Button_93 = 3093,
            Button_94 = 3094,
            Button_95 = 3095,
            Button_96 = 3096,
            Button_97 = 3097,
            Button_98 = 3098,
            Button_99 = 3099,
            Button_100 = 3100,
            Button_101 = 3101,
            Button_102 = 3102,
            Button_103 = 3103,
            Button_104 = 3104,
            Button_105 = 3105,
            Button_106 = 3106,
            Button_107 = 3107,
            Button_108 = 3108,
            Button_109 = 3109,
            Button_110 = 3110,
            Button_111 = 3111,
            Button_112 = 3112,
            Button_113 = 3113,
            Button_114 = 3114,
            Button_115 = 3115,
            Button_116 = 3116,
            Button_117 = 3117,
            Button_118 = 3118,
            Button_119 = 3119,
            Button_120 = 3120,
            Button_121 = 3121,
            Button_122 = 3122,
            Button_123 = 3123,
            Button_124 = 3124,
            Button_125 = 3125,
            Button_126 = 3126,
            Button_127 = 3127,
            Button_128 = 3128,
            Button_129 = 3129,
            Button_130 = 3130,
            Button_131 = 3131,
            Button_132 = 3132,
            Button_133 = 3133,
            Button_134 = 3134,
            Button_135 = 3135,
            Button_136 = 3136,
            Button_137 = 3137,
            Button_138 = 3138,
            Button_139 = 3139,
            Button_140 = 3140,
            Button_141 = 3141,
            Button_142 = 3142,
            Button_143 = 3143,
            Button_144 = 3144,
            Button_145 = 3145,
            Button_146 = 3146,
            Button_147 = 3147,
            Button_148 = 3148,
            Button_149 = 3149,
            Button_150 = 3150,
            Button_151 = 3151,
            Button_152 = 3152,
            Button_153 = 3153,
            Button_154 = 3154,
            Button_155 = 3155,
            Button_156 = 3156,
            Button_157 = 3157,
            Button_158 = 3158,
            Button_159 = 3159,
            Button_160 = 3160,
            Button_161 = 3161,
            Button_162 = 3162,
            Button_163 = 3163,
            Button_164 = 3164,
            Button_165 = 3165,
            Button_166 = 3166,
            Button_167 = 3167,
            Button_168 = 3168,
            Button_169 = 3169,
            Button_170 = 3170,
            Button_171 = 3171,
            Button_172 = 3172,
            Button_173 = 3173,
            Button_174 = 3174,
            Button_175 = 3175,
            Button_176 = 3176,
            Button_177 = 3177,
            Button_178 = 3178,
            Button_179 = 3179,
            Button_180 = 3180,
            Button_181 = 3181,
            Button_182 = 3182,
            Button_183 = 3183,
            Button_184 = 3184,
            Button_185 = 3185,
            Button_186 = 3186,
            Button_187 = 3187,
            Button_188 = 3188,
            Button_189 = 3189,
            Button_190 = 3190,
            Button_191 = 3191,
            Button_192 = 3192,
            Button_193 = 3193,
            Button_194 = 3194,
            Button_195 = 3195,
            Button_196 = 3196,
            Button_197 = 3197,
            Button_198 = 3198,
            Button_199 = 3199,
            Button_200 = 3200,
            Button_201 = 3201,
            Button_202 = 3202,
            Button_203 = 3203,
            Button_204 = 3204,
            Button_205 = 3205,
            Button_206 = 3206,
            Button_207 = 3207,
            Button_208 = 3208,
            Button_209 = 3209,
            Button_210 = 3210,
            Button_211 = 3211,
            Button_212 = 3212,
            Button_213 = 3213,
            Button_214 = 3214,
            Button_215 = 3215,
            Button_216 = 3216,
            Button_217 = 3217,
            Button_218 = 3218,
            Button_219 = 3219,
            Button_220 = 3220,
            Button_221 = 3221,
            Button_222 = 3222,
            Button_223 = 3223,
            Button_224 = 3224,
            Button_225 = 3225,
            Button_226 = 3226,
            Button_227 = 3227,
            Button_228 = 3228,
            Button_229 = 3229,
            Button_230 = 3230,
            Button_231 = 3231,
            Button_232 = 3232,
            Button_233 = 3233,
            Button_234 = 3234,
            Button_235 = 3235,
            Button_236 = 3236,
            Button_237 = 3237,
            Button_238 = 3238,
            Button_239 = 3239,
            Button_240 = 3240,
            Button_241 = 3241,
            Button_242 = 3242,
            Button_243 = 3243,
            Button_244 = 3244,
            Button_245 = 3245,
            Button_246 = 3246,
            Button_247 = 3247,
            Button_248 = 3248,
            Button_249 = 3249,
            Button_250 = 3250,
            Button_251 = 3251,
            Button_252 = 3252,
            Button_253 = 3253,
            Button_254 = 3254,
            Button_255 = 3255,
            Button_256 = 3256,
            Button_257 = 3257,
            Button_258 = 3258,
            Button_259 = 3259,
            Button_260 = 3260,
            Button_261 = 3261,
            Button_262 = 3262,
            Button_263 = 3263,
            Button_264 = 3264,
            Button_265 = 3265,
            Button_266 = 3266,
            Button_267 = 3267,
            Button_268 = 3268,
            Button_269 = 3269,
            Button_270 = 3270,
            Button_271 = 3271,
            Button_272 = 3272,
            Button_273 = 3273,
            Button_274 = 3274,
            Button_275 = 3275,
            Button_276 = 3276,
            Button_277 = 3277,
            Button_278 = 3278,
            Button_279 = 3279,
            Button_280 = 3280,
            Button_281 = 3281,
            Button_282 = 3282,
            Button_283 = 3283,
            Button_284 = 3284,
            Button_285 = 3285,
            Button_286 = 3286,
            Button_287 = 3287,
            Button_288 = 3288,
            Button_289 = 3289,
            Button_290 = 3290,
            Button_291 = 3291,
            Button_292 = 3292,
            Button_293 = 3293,
            Button_294 = 3294,
            Button_295 = 3295,
            Button_296 = 3296,
            Button_297 = 3297,
            Button_298 = 3298,
            Button_299 = 3299,
            Button_300 = 3300,
        }
    }
}
