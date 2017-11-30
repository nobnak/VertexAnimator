using UnityEngine;
using System.Collections;

namespace VertexAnimater {

    public static class ShaderConst {
        public const string SHADER_NAME = "VertexAnim/OneTime";
		public const string SHADER_ANIM_TEX = "_AnimTex";
		public const string SHADER_ANIM_TEX_TIME = "_AnimTex_T";
        public const string SHADER_SCALE = "_AnimTex_Scale";
        public const string SHADER_OFFSET = "_AnimTex_Offset";
        public const string SHADER_ANIM_END = "_AnimTex_AnimEnd";
        public const string SHADER_FPS = "_AnimTex_FPS";
        public const string SHADER_NORM_TEX = "_AnimTex_NormalTex";
    }
}
