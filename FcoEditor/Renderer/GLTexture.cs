using Hexa.NET.OpenGL;

namespace ConverseEditor.Rendering
{
    public class GlTexture
    {
        private uint m_Id = 0;
        public uint Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        public GlTexture()
        {

        }

        public GlTexture(nint in_Pixels, int in_Width, int in_Height)
        {
            GLSingle.Ins.GenTextures(1, ref m_Id);

            GLSingle.Ins.BindTexture(GLTextureTarget.Texture2D, Id);
            GLSingle.Ins.TexParameterf(GLTextureTarget.Texture2D, GLTextureParameterName.WrapS, (int)GLTextureWrapMode.Repeat);
            GLSingle.Ins.TexParameterf(GLTextureTarget.Texture2D, GLTextureParameterName.WrapT, (int)GLTextureWrapMode.Repeat);
            GLSingle.Ins.TexParameterf(GLTextureTarget.Texture2D, GLTextureParameterName.MinFilter, (int)GLTextureMinFilter.Linear);
            GLSingle.Ins.TexParameterf(GLTextureTarget.Texture2D, GLTextureParameterName.MagFilter, (int)GLTextureMagFilter.Linear);
            GLSingle.Ins.TexImage2D(GLTextureTarget.Texture2D, 0, GLInternalFormat.Rgba, in_Width, in_Height, 0, GLPixelFormat.Bgra, GLPixelType.UnsignedByte, in_Pixels);
        }

        public void Bind()
        {
            GLSingle.Ins.BindTexture(GLTextureTarget.Texture2D, m_Id);
        }

        public void Dispose()
        {
            GLSingle.Ins.DeleteTextures(1, ref m_Id);
        }
    }

}