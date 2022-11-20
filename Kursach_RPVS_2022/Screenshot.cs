using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Kursach_RPVS_2022
{
    static class Screenshot
    {
        public static Bitmap GetControlScreen(Control control)
        {
            //Запоминаем размеры контроллера
            Size szCurrent = control.Size;
            control.AutoSize = true;

            //Создание картинки нужного размера
            Bitmap bmp = new Bitmap(control.Width, control.Height);

            //Рисуем нужный скрин в bmp
            control.DrawToBitmap(bmp, control.ClientRectangle);

            //Возвращаем изначальные настройки
            control.AutoSize = false;
            control.Size = szCurrent;

            return bmp;
        }
    }
}
