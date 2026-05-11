using System.Data;
using System.Runtime.InteropServices;
using System.Text; //Для TXT
using Npgsql; //Для БД
using OfficeOpenXml;
using OfficeOpenXml.RichData; //Для Excel (и вроде PDF)

namespace WinForm
{
    public partial class Form1 : Form
    {
        public DataTable myTable = new DataTable();
        public Form1()
        {
            InitializeComponent();
            ExcelPackage.License.SetNonCommercialPersonal("Ilgam");
        }

        //Обработчик кнопки "Загрузить"
        private async void button1_Click(object sender, EventArgs e)
        {
            string connectionString =
                $"Server={textBox1.Text};Port={textBox2.Text};" +
                $"Database={textBox3.Text};User id={textBox4.Text};Password={textBox5.Text};";

            string query1 = "SELECT * FROM \"Ilgam\".\"TestTable\";";

            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Получаем данные из БД (С помощью Присоединённого режима)
                    // Или с помощью Отсоединённого режима.

                    // Присоединённый режим
                    using (var command = new NpgsqlCommand(query1, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            myTable.Clear();
                            myTable.Load(reader);
                        }
                        await command.ExecuteNonQueryAsync();
                    }

                    ////Отсоединённый режим
                    //using (var adapter = new NpgsqlDataAdapter(query1, connection))
                    //{
                    //    adapter.Fill(myTable);
                    //}
                }
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = myTable;
                dataGridView1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        //Кнопка "Очистить"
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                

                dataGridView1.DataSource = null;
                dataGridView1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //txt
            if (myTable == null || myTable.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            try
            {
                //Создание папки 'Data' в разделе bin/net.X.X проекта

                //1.Указываем путь ГДЕ будет создаваться папка 'Data'
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

                //2.Создаём папку по пути (выше)
                Directory.CreateDirectory(folder);

                //3.Шаблонно придумываем название TXT файла
                string fileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                //4.Создание ТХТ файла по пути (Путь до папки, название файла)
                string filePath = Path.Combine(folder, $"{fileName}");


                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    //Названия колонок
                    sw.WriteLine($"Отчёт за {DateTime.Now:yyyyMMdd_HHmmss}\n");

                    List<string> allColums = new List<string>();
                    List<string> currentRow = new List<string>();

                    //Обратился к таблице , к её колонкам (Ко всей 1й строке), нашёл все ячейки которые есть.
                    //Потом из каждой ячейки взял её название и поместил в список строк.
                    allColums = myTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();

                    //myTable.Colums - это обращение ко всем колонкам
                    //.Cast<DataColumn> - КАСТ , перебор , DataColumn - это тип данных , который описывает колонку в таблице
                    //.Select() , вызов foreach через метод. Column (переменная , это объект данных, где хранится ОДНА колонка , на которую сейчас
                    //смотрит ЦИКЛ foreach

                    //.ToList() - Каждую найденную колонку (вернее её название) , записывает в СПИСОК.

                    foreach (var columnName in allColums)
                    {
                        sw.Write($"|{columnName.ToString()}|" + "\t");
                    }

                    sw.Write("\n");

                    //Содержимое колонок
                    for (int i = 0; i < myTable.Rows.Count; i++)
                    {
                        for (int j = 0; j < myTable.Columns.Count; j++)
                        {
                            sw.Write($"|{myTable.Rows[i][j].ToString()}|" + "\t");
                        }
                        sw.WriteLine();
                    }
                }
                MessageBox.Show($"Данные успешно экспортированы в файл: {filePath}");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            //excel
            if (myTable == null || myTable.Rows.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }
            try
            {
                //Создание папки 'Data' в разделе bin/net.X.X проекта

                //1.Указываем путь ГДЕ будет создаваться папка 'Data'
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

                //2.Создаём папку по пути (выше)
                Directory.CreateDirectory(folder);

                //3.Шаблонно придумываем название EXCEL файла
                string fileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                //4.Создание EXCEL файла по пути (Путь до папки, название файла)
                string filePath = Path.Combine(folder, $"{fileName}");

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    //Создание ЛИСТА в EXCEL файле
                    var worksheet = package.Workbook.Worksheets.Add($"Отчёт о таблице TestTable");

                    worksheet.Cells["A1"].LoadFromDataTable(myTable, true);
                    package.SaveAs(new FileInfo(filePath));
                }
                MessageBox.Show($"Данные успешно экспортированы в файл: {filePath}");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }

        }
    }
}
