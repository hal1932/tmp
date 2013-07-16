using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.IO;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.D3DCompiler;
using System.Collections.Generic;
using SharpDX;


namespace psdview
{
    class BlenderImpl
    {
        private Device device = null;
        private Dictionary<psd.BlendMode, ComputeShader> shaderDic = null;


        public bool Setup()
        {
            this.device = new Device(DriverType.Hardware);
            var context = device.ImmediateContext;

            var shaderSource = "";
            using (var reader = new StreamReader(@"shader/blender.hlsl"))
            {
                shaderSource = reader.ReadToEnd();
            }

            this.shaderDic = new Dictionary<psd.BlendMode, ComputeShader>();
            for (int i = 0; i < (int)psd.BlendMode.Count; ++i)
            {
                var mode = (psd.BlendMode)i;
                var shader = this.CreateShader(this.device, shaderSource, mode.ToString());
                if (shader != null)
                {
                    this.shaderDic[mode] = shader;
                }
            }

            return true;
        }


        private ComputeShader CreateShader(Device device, string sourceCode, string entrypoint)
        {
            CompilationResult result = null;
            try
            {
                result = ShaderBytecode.Compile(sourceCode, entrypoint, "cs_5_0", ShaderFlags.EnableStrictness);
            }
            catch(CompilationException e)
            {
                System.Console.WriteLine(string.Format("[ERROR] failed to compile shader function: {0}", entrypoint));
                System.Console.WriteLine(e.Message);
                return null;
            }

            if (result.HasErrors)
            {
                System.Console.WriteLine(result.ResultCode.Code);
                System.Console.WriteLine(result.Message);
                return null;
            }

            var linkage = new ClassLinkage(device);
            return new ComputeShader(device, result.Bytecode, linkage);
        }
    }
}
