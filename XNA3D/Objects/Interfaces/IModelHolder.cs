using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace XNA3D.Objects.Interfaces
{
    public interface IModelHolder
    {
        bool HasModel { get; }
        bool HasTextures { get; }

        void setModel(Model model);
        Model getModel();
        void setModelTextures(Texture2D texture, Texture2D normal, Texture2D specular);
        Texture2D getTexture();
        Texture2D getNormalTexture();
        Texture2D getSpecularTexture();
    }
}
