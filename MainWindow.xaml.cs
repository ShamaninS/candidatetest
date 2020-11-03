using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json;
using System.Xml;

namespace TestTaskTransneft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Поля
        
        private List<CsvFileItem> _csvFile = new List<CsvFileItem>(); 
        private List<CsvFileItem> _tempCsvFile = new List<CsvFileItem>(); 
        private SelectionForm _selectionForm;
        private JsonFileItem _jsonFile = new JsonFileItem();
        private List<XmlFileItem> _xmlFile = new List<XmlFileItem>();

        #endregion
       
        public MainWindow()
        {
            InitializeComponent();
        }

        #region События
        
        /// <summary>
        /// Вызов диалогового окна для выбора файла .csv и его загрузки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchCsv_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = ".csv files|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                csvPath.Text = openFileDialog.FileName;
                
                try
                {
                    ReadCsv(csvPath.Text);
                }
                catch (Exception)
                {
                    // TODO: зайдет ли код в эту ветку, только при некорректной структуре файла?
                    // Зайдет при возникновении любой исключительной ситуации при чтении файла,
                    // но я предполагаю, что вероятнее всего это будет из-за некорректной структуры
                    MessageBox.Show("Проверьте структуру файла (Должно быть: Tag;Type;Address)", "Ошибка чтения .csv");
                }
            }
        }
        
        /// <summary>
        /// Вызов диалогового окна для выбора файла .json
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchJson_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = ".json files|*.json|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                    jsonPath.Text = openFileDialog.FileName;
                    
                    try
                    {
                        ReadJson(jsonPath.Text);
                    }
                    catch (Exception)
                    {
                        // TODO: зайдет ли код в эту ветку, только при некорректной структуре файла?
                        // Зайдет при возникновении любой исключительной ситуации при чтении файла,
                        // но я предполагаю, что вероятнее всего это будет из-за некорректной структуры
                        MessageBox.Show("Проверьте структуру файла (Должно быть {\"TypeInfos\":[{\"TypeName\": \"ZRP\",\"Propertys\": {\"flowkD\": " +
                                        "\"double\"}},{\"TypeName\": \"AI\",\"Propertys\": {\"Cmd\": \"double\"}}]}", "Ошибка чтения .json");
                    }
            }
        }
        
        /// <summary>
        /// Запуск программы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartProgram_Click(object sender, RoutedEventArgs e)
        {
            if (csvPath.Text != string.Empty && jsonPath.Text!=string.Empty)
            {
                _xmlFile.Clear(); // Очистка содержимого XML файла, в случае повторного запуска
                _tempCsvFile.Clear(); // Очистка содержимого листа, содержащего строки csv файла с параметром IsCheked = true, в случае повторного запуска
                for (int i = 0; i < _csvFile.Count; i++) // Генерация нового листа, содержащего строки csv файла с параметром IsCheked = true
                {
                    if (_csvFile[i].isChecked) _tempCsvFile.Add(_csvFile[i]);
                }
                try
                {  
                    MergeringCsvAndJson();
                }
                catch (Exception)
                {
                    MessageBox.Show("Что-то пошло не так", "Ошибка");
                }

                string pathToXml = PathToXml();
                if (pathToXml != string.Empty) SaveXML(pathToXml, _xmlFile);
            }
            else
            {
                MessageBox.Show("Файлы не выбраны","Ошибка");
            }
        }
        
        /// <summary>
        /// Вызов формы для удаления ненужных строк из .csv файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallSelectionForm_Click(object sender, RoutedEventArgs e)
        {
            if (csvPath.Text!=string.Empty)
            {
                _selectionForm = new SelectionForm(_csvFile);
                _selectionForm.Apply.Click += Apply_OnClick;

                if (_selectionForm.ShowDialog() == true)
                {
                    _selectionForm.Show();
                }
            }
        }
        
        /// <summary>
        /// Метод, вызываемый при нажатии кнопки "Применить" в форме SelectionForm.
        /// Обновляет информацию о строках .csv файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Apply_OnClick(object sender, RoutedEventArgs e)
        {
            _csvFile.Clear();
            _csvFile = _selectionForm.ReturnNewList();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Чтение содержимого файла .csv
        /// </summary>
        public void ReadCsv(string path)
        {
            StreamReader streamReader = new StreamReader(path);
            while (!streamReader.EndOfStream)
            {
                _csvFile.Add(new CsvFileItem(streamReader.ReadLine()));
            }
            streamReader.Close();
            _csvFile.Remove(_csvFile[0]);
        }

        /// <summary>
        /// Чтение содержимого файла .json
        /// </summary>
        private void ReadJson(string path)
        {
            string jsonString = File.ReadAllText(path);
            _jsonFile = JsonSerializer.Deserialize<JsonFileItem>(jsonString);
        }
        
        /// <summary>
        /// Метод компанует строки из файла .csv c файлом .json и осуществляет преобразования
        /// </summary>
        private void MergeringCsvAndJson()
        {
            int lastValue = 0;
            bool firstTime = true;
            for (int i = 0; i < _tempCsvFile.Count; i++)
            {
                for (int j = 0; j < _jsonFile.TypeInfos.Count; j++)
                {
                    if (_tempCsvFile[i].Type == _jsonFile.TypeInfos[j].TypeName)
                    {
                        string[] tempKeys = new string[_jsonFile.TypeInfos[j].Propertys.Keys.Count];
                        string[] tempValue = new string[_jsonFile.TypeInfos[j].Propertys.Keys.Count];
                    
                        _jsonFile.TypeInfos[j].Propertys.Keys.CopyTo(tempKeys,0);
                        _jsonFile.TypeInfos[j].Propertys.Values.CopyTo(tempValue,0);
                        
                        int[] value = new int[tempValue.Length];
                        
                        
                        // TODO: требуется пояснить нобходимость передачи массива с использованием ref
                        // Для того, чтобы не возвращать результат работы метода, а производить преобразования в исходном массиве
                        ReformStringToByte(tempValue,ref value);
                        CalcAmount(ref value,ref lastValue,ref firstTime);
                        
                        XmlFileItem xmlFileItem = new XmlFileItem();
                        
                        for (int k = 0; k < tempValue.Length; k++)
                        {
                            xmlFileItem.Dictionary.Add(_tempCsvFile[i].Name+"."+tempKeys[k],value[k]);
                        }
                        _xmlFile.Add(xmlFileItem);
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Метод преобразует строковое наименование типа данных, в байтовый размер
        /// </summary>
        /// <param name="tempValueStrings">Массив с наименованиями типов данных</param>
        /// <param name="value">Массив с байтовыми размерами соответсвующего типа данных</param>
        private void ReformStringToByte(string[] tempValueStrings,ref int[] value)
        {
            for (int i = 0; i < tempValueStrings.Length; i++)
            {
                if (tempValueStrings[i].Equals("int")) value[i] = 4;
                if (tempValueStrings[i].Equals("double")) value[i] = 8;
                if (tempValueStrings[i].Equals("bool")) value[i] = 1;
            }
        }
        
        /// <summary>
        /// Метод реализует смещение байтового размера каждой строки на байтовый размер предыдущей строки 
        /// </summary>
        /// <param name="value">Массив с байтовыми размерами соответсвующего типа данных</param>
        /// <param name="lastValue">Последнее значение байтого смещения строки .csv файла</param>
        /// <param name="firstTime">Разделяет логику подсчета байтого смещения для первой строки и последующих</param>
        private void CalcAmount(ref int[] value, ref int lastValue, ref bool firstTime)
        {
            int[] temp = new int[value.Length];
            value.CopyTo(temp,0);

            if (firstTime)
            {
                for (int i = 1; i < temp.Length; i++)
                {
                    temp[i] += temp[i - 1];
                }
                
                for (int i = 1; i < value.Length; i++)
                {
                    value[i] = temp[i-1];
                }
                value[0] = 0;
                firstTime = false;
            }
            else
            {
                for (int i = 1; i < temp.Length; i++)
                {
                    temp[0] = lastValue + value[0];
                    temp[i] += temp[i - 1];
                }
                for (int i = 0; i < value.Length; i++)
                {
                    value[i] = temp[i];
                }
            }
            
            lastValue = value[value.Length - 1];
        }
        
        /// <summary>
        /// Сохранение в файл .xml по шаблону
        /// </summary>
        /// <param name="path">Путь до файла</param>
        /// <param name="list">Лист всех словарей содержащих конечные значения для вывода</param>
        private void SaveXML(string path, List<XmlFileItem> list)
        {
            var doc = new XmlDocument();

            var root = doc.CreateElement("root");

            doc.AppendChild(root);
            
            for (int i = 0; i < list.Count; i++)
            {
                string[] tempKeys = new string[list[i].Dictionary.Count];
                int[] tempValues = new int[list[i].Dictionary.Count];
                    
                list[i].Dictionary.Keys.CopyTo(tempKeys,0);
                list[i].Dictionary.Values.CopyTo(tempValues,0);
                    
                for (int k = 0; k < tempKeys.Length; k++)
                {
                    var item = doc.CreateElement("item");
                    var attribute = doc.CreateAttribute("Binding");
                    attribute.Value = "Introduced";
                    item.Attributes.Append(attribute);

                    var node = doc.CreateElement("node-path");
                    var address = doc.CreateElement("address");
                    node.InnerText = $"{tempKeys[k]}";
                    address.InnerText = $"{tempValues[k]}";

                    root.AppendChild(item);
                    item.AppendChild(node);
                    item.AppendChild(address);
                }
            }
            doc.Save(path);
        }

        /// <summary>
        /// Выбор пути для сохранения файла .xml
        /// </summary>
        /// <returns>Путь к месту сохранения</returns>
        private string PathToXml()
        {
            string path = string.Empty;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Output";
            saveFileDialog.DefaultExt = ".xml";
            if (saveFileDialog.ShowDialog() == true)
            {
                path = saveFileDialog.FileName;
            }
            if (path != string.Empty) MessageBox.Show($"Файл сохранен по пути {path}", "Успешно");
            return path;
        }

        #endregion
    }
}