using System.IO;
using System.Xml.Serialization;

namespace SoundPanel
{

    // Class for writing and reading serialized xml files. This is used for presets
    public class XmlManager
    {
        public static void XmlDataWriter(object obj, string filename)
        {
            XmlSerializer sr = new XmlSerializer(obj.GetType());
            TextWriter writer = new StreamWriter(filename);
            sr.Serialize(writer, obj);
            writer.Close();
        }

        public static Data XmlDataReader(string filename)
        {
            Data obj = new Data();
            XmlSerializer xs = new XmlSerializer(typeof(Data));
            FileStream reader = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            obj = (Data)xs.Deserialize(reader);
            reader.Close();
            return obj;
        }
    }
}
