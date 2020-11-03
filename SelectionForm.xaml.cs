using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TestTaskTransneft
{
    /// <summary>
    /// Логика взаимодействия для dgSelectionForm.xaml
    /// </summary>
    public partial class SelectionForm : Window
    {
        #region Поля

        private List<CsvFileItem> _list;
        
        #endregion
        
        #region Конструктор

        public SelectionForm(List<CsvFileItem> list)
        {
            InitializeComponent();
            
            _list = new List<CsvFileItem>();

            foreach (var element in list)
            {
                CsvFileItem csvFileItem = new CsvFileItem(";;");
                csvFileItem = element.Clone() as CsvFileItem;
                _list.Add(csvFileItem); 
            }
            
            Apply.Click += Apply_OnClick;
            Cancel.Click += Cancel_OnClick;
            dgSelectionForm.MouseDown += DgSelectionForm_OnSelected;
            dgSelectionForm.SelectionChanged += DgSelectionFormOnSelectionChanged;
        }

        
        #endregion

        #region События

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dgSelectionForm.ItemsSource = _list;
        }

        public void Apply_OnClick(object sender, RoutedEventArgs e)
        {
            Apply.Click -= Apply_OnClick;
            Cancel.Click -= Cancel_OnClick;
            dgSelectionForm.MouseDown -= DgSelectionForm_OnSelected;
            dgSelectionForm.SelectionChanged -= DgSelectionFormOnSelectionChanged;

            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Apply.Click -= Apply_OnClick;
            Cancel.Click -= Cancel_OnClick;
            dgSelectionForm.MouseDown -= DgSelectionForm_OnSelected;
            dgSelectionForm.SelectionChanged -= DgSelectionFormOnSelectionChanged;
 
            Close();
        }

        #endregion

        /// <summary>
        /// Метод, вызываемый при клике по активной строке DataGrid. Позволяет установить/снять галочку нажатием по строке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgSelectionForm_OnSelected(object sender, RoutedEventArgs e)
        {
            ((CsvFileItem) dgSelectionForm.CurrentItem).isChecked = !((CsvFileItem) dgSelectionForm.CurrentItem).isChecked;
            dgSelectionForm.Items.Refresh();
        }
        
        private void DgSelectionFormOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((CsvFileItem) dgSelectionForm.CurrentItem).isChecked = !((CsvFileItem) dgSelectionForm.CurrentItem).isChecked;
            dgSelectionForm.Items.Refresh();
        }

        
        /// <summary>
        /// Главный Toggle элемент, управляющий CheckBox в DataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Toggle_OnClick(object sender, RoutedEventArgs e)
        {
            if (Toggle.IsChecked == true)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    _list[i].isChecked = true;
                    dgSelectionForm.Items.Refresh();
                } 
            }
            else
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    _list[i].isChecked = false;
                    dgSelectionForm.Items.Refresh();
                } 
            }
        }
        
        /// <summary>
        /// Метод, возвращающий лист всех строк .csv файла с изменениями
        /// </summary>
        /// <returns>Лист, содержащий полный список строк .csv файла с сохраненными изменениями</returns>
        public List<CsvFileItem> ReturnNewList()
        {
            return _list;
        }
    }
}