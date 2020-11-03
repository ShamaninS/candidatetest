using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestTaskTransneft
{
    /// <summary>
    /// Класс, содержащий данные каждой строки csv файла
    /// </summary>
    public class CsvFileItem : ICloneable
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool isChecked { get; set; }
        
        public CsvFileItem(string n)
        {
            string[] temp = n.Split(';');
            Name = temp[0];
            Type = temp[1];
            isChecked = true;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    
}