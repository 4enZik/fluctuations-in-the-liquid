using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Kursach_RPVS_2022
{
    class Serializer
    {
        public static void SaveSeans(int formColor, int tabColor)
        {
            
            List<int> SaveInfo = new List<int> { formColor, tabColor };

            XmlSerializer formatter = new XmlSerializer(typeof(List<int>));

            using (FileStream fs = new FileStream("SaveInfo.xml", FileMode.Truncate))
            {
                formatter.Serialize(fs, SaveInfo);
            }
        }
        public static List<int> Download()
        {

            List<int> DownloadInfo = new List<int>();
            XmlSerializer formatter = new XmlSerializer(typeof(List<int>));
            using (FileStream fs = new FileStream("SaveInfo.xml", FileMode.Open))
            {
                DownloadInfo = formatter.Deserialize(fs) as List<int>;

            }
            return DownloadInfo;
        }
    }
}
