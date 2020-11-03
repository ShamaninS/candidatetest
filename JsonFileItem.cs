using System.Collections.Generic;
using System.Reflection;

namespace TestTaskTransneft
{
    /// <summary>
    /// Класс описывающий структуру .json файла для его десериализации
    /// </summary>
    public class JsonFileItem
    {
        public List<TypeInformation> TypeInfos { get; set; }
        
        public JsonFileItem()
        {
            TypeInfos = new List<TypeInformation>();
        }
    }

    public class TypeInformation
    {
        public string TypeName { get; set; }
        public Dictionary<string, string> Propertys { get; set; }

        public TypeInformation()
        {
            Propertys = new Dictionary<string, string>();
        }
    }

}