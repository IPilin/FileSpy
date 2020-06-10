using System;
using System.Drawing;

namespace FileSpy.Classes
{
    class FileData
    {
        public Icon ICO { get; set; }

        public FileData()
        {
            using (var iconStream = System.Windows.Application.GetResourceStream(new Uri("Resources/magnet.ico", UriKind.Relative)).Stream)
            {
                ICO = new Icon(iconStream);
            }
        }
    }
}
