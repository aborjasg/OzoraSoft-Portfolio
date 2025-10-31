using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.Library.PictureMaker
{
    public interface IPictureEngine
    {
        void DrawPictureTitle();
        string MakePicture();
    }
}
