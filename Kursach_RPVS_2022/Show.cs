using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace Kursach_RPVS_2022
{
    public partial class Show : Form
    {
        #region Параметры
        // Параметры поворота и приближения
        double xRot = 0, yRot = 0, zRot = 0;
        float zoom = 0;
        //Объект физики
        Phisics_model phisics_model;
        //Рассчётные переменные
        double time_right = 0;
        double time_left = 0;

        double coord_static;
        double coord_non_static;

        string right_liquid = "Вода";
        string left_liquid = "Вода";

        double ro_water = 1000;
        double ro_rtut = 1360;
        double ro_spirt = 790;
        //Параметры пробки
        double ro_prob = 200;
        double size_of_prob = 1;
        double H_prob = 0.5;
        //Изменения колб
        int count_change_1 = 0;
        int count_change_2 = 0;
        //Изменение сохранения
        int count_change_save = 0;

        #endregion

        public Show()
        {
            InitializeComponent();
            phisics_model = new Phisics_model();
            chart1.Series[0].Points.AddXY(10, 10);
            
            List<int> colors = Serializer.Download();
            Color formColor = Color.FromArgb(colors[0]);
            Color tabColor = Color.FromArgb(colors[1]);
            this.BackColor = formColor;
            tabPage1.BackColor = tabColor;
            tabPage2.BackColor = tabColor;
            groupBox3.BackColor = tabColor;
        }
        #region Кнопки управления стандартными процессами
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            Draw_graphic_1();
        }
        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        #region Справка об программе
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, @"C:\Users\user\source\repos\Kursach_RPVS_2022\Kursach_RPVS_2022\Helper.chm");
        }
        #endregion

        #region Изменение цвета формы!
        private void заднийЦветФормыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.ShowDialog();
            Color color = colorDialog.Color;
            this.BackColor = color;
            
        }

        private void цветВкладокПрограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.ShowDialog();
            Color color = colorDialog.Color;
            tabPage1.BackColor = color;
            tabPage2.BackColor = color;
            groupBox3.BackColor = color;

        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Serializer.SaveSeans(this.BackColor.ToArgb(), tabPage1.BackColor.ToArgb());
        }

        #endregion

        #region Справки для пользователя
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            
            MessageBox.Show(
                            "W - Поворот камеры вверх" + Environment.NewLine +
                            "S - Поворот камеры вниз" + Environment.NewLine +
                            "A - Поворот камеры влево" + Environment.NewLine +
                            "D - Поворот камеры вправо" + Environment.NewLine +
                            "Z - Приближения камеры" + Environment.NewLine +
                            "X - Отдаление камеры" + Environment.NewLine
                            , "Горячие клавиши");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Если ниодна из галочек не будет включена, то файл сохраниться пустым!!" + Environment.NewLine + Environment.NewLine +
                            "График возможно сохранить только в Word!!" + Environment.NewLine + Environment.NewLine +
                            "В строке пути к файлу, указать вконце имя !БЕЗ РАСШИРЕНИЯ!", "Справка о сохранении");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Если у вас не получается управлять кнопками, попробуйте нажать ЛКМ по элементу отображения анимации и использовать горячие клавиши!!!", "Справка об управлении");

        }

        private void button9_Click(object sender, EventArgs e)
        {
            MessageBox.Show("После изменения данных о пробке, обязательно нажмите кнопку \"Применить\"!!" + Environment.NewLine + Environment.NewLine +
                            "!!!ВНИМАНИЕ!!! Данные пробкии введённые вами -- не влияют на её размервы в отображении. Они влияют только на формульные данные.", "Справка о изменении пробки");
        }
        #endregion
        
        #region Управление анимации
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            zoom = trackBar1.Value;
        }

        private void Rotate_btn_left_Click(object sender, EventArgs e)
        {
            yRot += 1;
        }

        private void Rotate_btn_right_Click(object sender, EventArgs e)
        {
            yRot -= 1;
        }

        private void Rotate_btn_up_Click(object sender, EventArgs e)
        {
            zRot -= 1;
        }

        private void Rotate_btn_down_Click(object sender, EventArgs e)
        {
            zRot += 1;
        }

        private void glControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                yRot += 1;
            }
            if (e.KeyCode == Keys.D)
            {
                yRot -= 1;
            }
            if (e.KeyCode == Keys.W)
            {
                zRot -= 1;
            }
            if (e.KeyCode == Keys.S)
            {
                zRot += 1;
            }
            if (e.KeyCode == Keys.Z)
            {
                zoom += 1;
            }
            if (e.KeyCode == Keys.X)
            {
                zoom -= 1;
            }
        }
        #endregion

        #region Рисование графика
        public void Draw_graphic_1()
        {
            double a = 0;
            double b = Convert.ToDouble(Max.Text);
            double h = 0;
            try
            {
                h = Convert.ToDouble(Step.Text);
            }
            catch
            {
                MessageBox.Show("Поменяёте , на .");
            }
            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();
            chart1.Series[3].Points.Clear();
            double t = a;
            double x;
            while(t <= b)
            {
                if (nez_water.Checked)
                {
                    
                    x = Calculate_sdvig(ro_water, t, ro_prob);
                    chart1.Series[0].Points.AddXY(t, x);
                    x = 0;
                }
                if (nez_hg.Checked)
                {
                    x = Calculate_sdvig(ro_rtut, t, ro_prob);
                    chart1.Series[1].Points.AddXY(t, x);
                    x = 0;
                }
                if (zat_water.Checked)
                {
                    x = Calculate_sdvig_nez(ro_water, t, ro_prob, size_of_prob);
                    chart1.Series[2].Points.AddXY(t, x);
                    x = 0;
                }
                if (zat_C2H5OH.Checked)
                {
                    x = Calculate_sdvig_nez(ro_spirt, t, ro_prob, size_of_prob);
                    chart1.Series[3].Points.AddXY(t, x);
                    x = 0;
                }
                t += h;
            }
        }
        #endregion

        #region Рассчёт сдвигов для обеих систем / графиков
        public double Calculate_sdvig(double ro, double t, double ro_prob)
        {
            double x = phisics_model.Calculate_static(ro, t, ro_prob, H_prob);
            return x;
        }

        public double Calculate_sdvig_nez(double ro, double t, double ro_prob, double size_of_prob)
        {
            double x = phisics_model.Calculate_not_static(ro, t, ro_prob, size_of_prob, H_prob);
            return x;
        }
        #endregion

        #region Основной цикл рассчёта и рисования
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            time_right += 0.1;
            time_left += 0.1;
            if(time_left > 20)
            {
                count_change_1 += 1;
                if(Math.IEEERemainder(count_change_1,2) == 0)
                {
                    left_liquid = "Вода";
                }
                else
                {
                    left_liquid = "Ртуть";
                }
                time_left = 0;
            }
            if(time_right > 25)
            {
                count_change_2 += 1;
                if (Math.IEEERemainder(count_change_2, 2) == 0)
                {
                    right_liquid = "Вода";
                }
                else
                {
                    right_liquid = "Спирт";
                }
                time_right = 0;
            }
            calculate_coords();
            Draw();
        }
        #endregion

        #region Вычисление координат
        public void calculate_coords()
        {
            if(left_liquid == "Вода")
            {
                coord_static = phisics_model.Calculate_static(ro_water, time_left, ro_prob, H_prob);
                coord_static = coord_static * 10;
            }
            else // Рассчитываем координаты для ртути
            {
                coord_static = phisics_model.Calculate_static(ro_rtut, time_left, ro_prob, H_prob);
                coord_static = coord_static * 10;
            }

            if(right_liquid == "Вода")
            {
                coord_non_static = phisics_model.Calculate_not_static(ro_water, time_right, ro_prob, size_of_prob, H_prob);
                coord_non_static = coord_non_static * 10;
            }
            else //Рассчитываем координаты для спирта
            {
                coord_non_static = phisics_model.Calculate_not_static(ro_spirt, time_right, ro_prob, size_of_prob, H_prob);
                coord_non_static = coord_non_static * 10;
            }
        }
        #endregion

        #region Обновление данных о пробке
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                ro_prob = Convert.ToDouble(textBox1.Text);
                size_of_prob = Convert.ToDouble(textBox2.Text);
                H_prob = Convert.ToDouble(textBox3.Text);
            }
            catch
            {
                MessageBox.Show("Введены некорректные данные");
            }
        }
        #endregion

        #region Сохранение в Word
        private void button5_Click(object sender, EventArgs e)
        {

            var application = new Word.Application();
            Word.Document document = application.Documents.Add();

            
            if (nez_water.Checked)
            {
                WriteNewTitle(document, "Незатухающие колебания в вода");
                WriteNewTable(document, 21, 6, ro_water, 1);
                Word.Paragraph userParagrapth = document.Paragraphs.Add();
            }
            if (nez_hg.Checked)
            {
                WriteNewTitle(document, "Незатухающие колебания в ртути");
                WriteNewTable(document, 21, 6, ro_rtut, 1);
                Word.Paragraph userParagrapth = document.Paragraphs.Add();
            }
            if (zat_water.Checked)
            {
                WriteNewTitle(document, "Затухающие колебания в воде");
                WriteNewTable(document, 26, 6, ro_water, 2);
                Word.Paragraph userParagrapth = document.Paragraphs.Add();
            }
            if (zat_C2H5OH.Checked)
            {
                WriteNewTitle(document, "Затухающие колебания в спирте");
                WriteNewTable(document, 26, 6, ro_spirt, 2);
                Word.Paragraph userParagrapth = document.Paragraphs.Add();
            }


            //Сохраняем скриншот графика в файле 
            if (checkBox1.Checked)
            {
                Bitmap bmp = Screenshot.GetControlScreen(chart1);
                bmp.Save(System.IO.Directory.GetCurrentDirectory() + @"\image.png");
                Word.Paragraph ImagePar = document.Paragraphs.Add();
                Word.Range ImageRange = ImagePar.Range;
                Word.InlineShape image = ImageRange.InlineShapes.AddPicture(System.IO.Directory.GetCurrentDirectory() + @"\image.png");

                System.IO.File.Delete(System.IO.Directory.GetCurrentDirectory() + @"\image.png");
                bmp.Dispose();
            }
            //

            document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);

            application.Visible = true;

            document.SaveAs2(textBox4.Text+".docx");
            document.SaveAs2(textBox4.Text+ ".pdf", Word.WdExportFormat.wdExportFormatPDF);
            //}
        }

        public void WriteNewTitle(Word.Document document, string name_title)
        {
            //Создание заголовка страницы!
            Word.Paragraph userParagrapth = document.Paragraphs.Add();
            Word.Range userRange = userParagrapth.Range;
            userRange.Text = name_title;
            //TODO Доделать красивый формат текста, а так же вствку скрина с графиком!
            
            userRange.InsertParagraphAfter();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            count_change_save++;
            if(count_change_save % 2 != 0)
            {
                MessageBox.Show("Перед сохранением, убедитесь что вы отрисовали нужный вам график!");
            }
        }



        public void WriteNewTable(Word.Document document, int row, int col, double ro_liquid, int choice)
        {
            //Создание новой таблицы
            Word.Paragraph tableParagraph = document.Paragraphs.Add();
            Word.Range tableRange = tableParagraph.Range;
            Word.Table paymentsTable = document.Tables.Add(tableRange, row, col);
            paymentsTable.Borders.InsideLineStyle = paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
            paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

            Word.Range cellRange;
            //Заполнение первой строки
            cellRange = paymentsTable.Cell(1, 1).Range;
            cellRange.Text = "Смещение (м)";
            cellRange = paymentsTable.Cell(1, 2).Range;
            cellRange.Text = "Время (c)";
            cellRange = paymentsTable.Cell(1, 3).Range;
            cellRange.Text = "Плотность пробки (кг/м^3)";
            cellRange = paymentsTable.Cell(1, 4).Range;
            cellRange.Text = "Площадь дна пробки (м^2)";
            cellRange = paymentsTable.Cell(1, 5).Range;
            cellRange.Text = "Плотность жидксоти (кг/м^3)";
            cellRange = paymentsTable.Cell(1, 6).Range;
            cellRange.Text = "Высота пробки (м)";

            paymentsTable.Rows[1].Range.Bold = 1;
            paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

            //Заполнение второй и последующих строк
            cellRange = paymentsTable.Cell(2, 3).Range;
            cellRange.Text = Convert.ToString(ro_prob);
            cellRange = paymentsTable.Cell(2, 4).Range;
            cellRange.Text = Convert.ToString(size_of_prob);
            cellRange = paymentsTable.Cell(2, 5).Range;
            cellRange.Text = Convert.ToString(ro_liquid);
            for (int i = 2; i <= row; i++)
            {
                if(choice == 1)
                {
                    cellRange = paymentsTable.Cell(i, 1).Range;
                    double x = phisics_model.Calculate_static(ro_liquid, i-1, ro_prob, H_prob);
                    cellRange.Text = Convert.ToString(x);
                    cellRange = paymentsTable.Cell(i, 2).Range;
                    cellRange.Text = Convert.ToString(i-1);
                }
                if(choice == 2)
                {
                    cellRange = paymentsTable.Cell(i, 1).Range;
                    double x = phisics_model.Calculate_not_static(ro_liquid, i - 1, ro_prob, size_of_prob, H_prob);
                    cellRange.Text = Convert.ToString(x);
                    cellRange = paymentsTable.Cell(i, 2).Range;
                    cellRange.Text = Convert.ToString(i - 1);
                }
            }
        }
        #endregion

        #region Сохранение в Excel
        private void button6_Click(object sender, EventArgs e)
        {
            var application = new Excel.Application();
            Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

            
            if (nez_water.Checked)
            {
                Excel.Worksheet worksheet = application.Worksheets.Add();
                worksheet = application.Worksheets.Item[1];
                worksheet.Name = "Незат. колебания в воде";
                WriteNewHeaderTableExcel(worksheet);
                WriteNewLowerTableExcel(worksheet, ro_water, 1, 20);
                worksheet.Columns.AutoFit();
            }
            if (nez_hg.Checked)
            {
                Excel.Worksheet worksheet = application.Worksheets.Add();
                worksheet = application.Worksheets.Item[1];
                worksheet.Name = "Незат. колебания в ртути";
                WriteNewHeaderTableExcel(worksheet);
                WriteNewLowerTableExcel(worksheet, ro_rtut, 1, 20);
                worksheet.Columns.AutoFit();
            }
            if (zat_water.Checked)
            {
                Excel.Worksheet worksheet = application.Worksheets.Add();
                worksheet = application.Worksheets.Item[1];
                worksheet.Name = "Зат. колебания в воде";
                WriteNewHeaderTableExcel(worksheet);
                WriteNewLowerTableExcel(worksheet, ro_water, 2, 25);
                worksheet.Columns.AutoFit();
            }
            if (zat_C2H5OH.Checked)
            {
                Excel.Worksheet worksheet = application.Worksheets.Add();
                worksheet = application.Worksheets.Item[1];
                worksheet.Name = "Зат. колебания в спирте";
                WriteNewHeaderTableExcel(worksheet);
                WriteNewLowerTableExcel(worksheet, ro_spirt, 2, 25);
                worksheet.Columns.AutoFit();
            }
            application.Visible = true;
            workbook.SaveAs(textBox4.Text +".xlsx");
        }

        public void WriteNewHeaderTableExcel(Excel.Worksheet worksheet)
        {
            worksheet.Cells[1][1] = "Смещение (м)";
            worksheet.Cells[2][1] = "Время (с)";
            worksheet.Cells[3][1] = "Плотность пробки (кг/м^3)";
            worksheet.Cells[4][1] = "Площадь дна пробки (м^2)";
            worksheet.Cells[5][1] = "Плотность жидкости(кг/м^3)";
            worksheet.Cells[6][1] = "Высота пробки (м)";

            Excel.Range headerRange = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[6][1]];
            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            headerRange.Font.Italic = true;
        }



        public void WriteNewLowerTableExcel(Excel.Worksheet worksheet, double ro_liquid, int choice, int row)
        {
            worksheet.Cells[3][2] = Convert.ToString(ro_prob);
            worksheet.Cells[4][2] = Convert.ToString(size_of_prob);
            worksheet.Cells[5][2] = Convert.ToString(ro_liquid);
            worksheet.Cells[6][2] = Convert.ToString(H_prob);

            for (int i = 2; i <= row; i++)
            {
                if (choice == 1)
                {
                    double x = phisics_model.Calculate_static(ro_liquid, i - 1, ro_prob, H_prob);
                    worksheet.Cells[1][i] = Convert.ToString(x);
                    worksheet.Cells[2][i] = Convert.ToString(i - 1);
                }
                if (choice == 2)
                {
                    double x = phisics_model.Calculate_not_static(ro_liquid, i - 1, ro_prob, size_of_prob, H_prob);
                    worksheet.Cells[1][i] = Convert.ToString(x);
                    worksheet.Cells[2][i] = Convert.ToString(i - 1);
                }
            }
        }
        #endregion

        #region Рисование OpenGL
        private void Draw()
        {
            glControl1.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(1.04f, 4 / 3, 1, 10000);
            Matrix4 lookat = Matrix4.LookAt(100+zoom, 80+zoom, 0, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.LoadMatrix(ref perspective);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.LoadMatrix(ref lookat);
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Rotate(xRot, 1, 0, 0);
            GL.Rotate(yRot, 0, 1, 0);
            GL.Rotate(zRot, 0, 0, 1);
            GL.Begin(BeginMode.Quads);
            //ПРАВАЯ КОЛБА
            // Низ правой колбы 
            GL.Color3(Color.Gray);
            GL.Vertex3(0, 0, -50);
            GL.Vertex3(0, 0, -25);
            GL.Vertex3(25, 0, -25);
            GL.Vertex3(25, 0, -50);
            //дальняя стенка прав колбы
            GL.Vertex3(0, 0, -50);
            GL.Vertex3(0, 25, -50);
            GL.Vertex3(0, 25, -25);
            GL.Vertex3(0, 0, -25);
            //левая стенка правой колбы
            GL.Vertex3(0, 0, -25);
            GL.Vertex3(0, 25, -25);
            GL.Vertex3(25, 25, -25);
            GL.Vertex3(25, 0, -25);
            //ближняя стенка правой колбы
            GL.Vertex3(25, 0, -25);
            GL.Vertex3(25, 0, -50);
            GL.Vertex3(25, 25, -50);
            GL.Vertex3(25, 25, -25);
            //правая сторона правой колбы
            GL.Vertex3(25, 0, -50);
            GL.Vertex3(0, 0, -50);
            GL.Vertex3(0, 25, -50);
            GL.Vertex3(25, 25, -50);

            //ЛЕВАЯ КОЛБА
            GL.Color3(Color.Red);
            //Низ левой колбы
            GL.Vertex3(0, 0, 25);
            GL.Vertex3(25, 0, 25);
            GL.Vertex3(25, 0, 50);
            GL.Vertex3(0, 0, 50);
            //Дальняя стенка левой колбы
            GL.Vertex3(0, 0, 25);
            GL.Vertex3(0, 25, 25);
            GL.Vertex3(0, 25, 50);
            GL.Vertex3(0, 0, 50);
            //Левая стенка левой колбы
            GL.Vertex3(0, 0, 50);
            GL.Vertex3(0, 25, 50);
            GL.Vertex3(25, 25, 50);
            GL.Vertex3(25, 0, 50);
            //Правая стенка левой колбы
            GL.Vertex3(0, 0, 25);
            GL.Vertex3(0, 25, 25);
            GL.Vertex3(25, 25, 25);
            GL.Vertex3(25, 0, 25);
            //Передняя стенка левой колбы
            GL.Vertex3(25, 0, 25);
            GL.Vertex3(25, 0, 50);
            GL.Vertex3(25, 25, 50);
            GL.Vertex3(25, 25, 25);
            GL.End();

            GL.LineWidth(3);
            GL.Color3(Color.Black);
            GL.Begin(BeginMode.Lines);
            //Нижние линии правой колбы
            GL.Vertex3(0, 0, -50);
            GL.Vertex3(0, 0, -25);

            GL.Vertex3(0, 0, -25);
            GL.Vertex3(25, 0, -25);

            GL.Vertex3(25, 0, -25);
            GL.Vertex3(25, 0, -50);

            GL.Vertex3(25, 0, -50);
            GL.Vertex3(0, 0, -50);
            //Вертикальные линии правой колбы
            GL.Vertex3(0, 0, -50);
            GL.Vertex3(0, 25, -50);

            GL.Vertex3(0, 0, -25);
            GL.Vertex3(0, 25, -25);

            GL.Vertex3(25, 0, -50);
            GL.Vertex3(25, 25, -50);

            GL.Vertex3(25, 0, -25);
            GL.Vertex3(25, 25, -25);
            //Верхние линии правой колбы
            GL.Vertex3(0, 25, -25);
            GL.Vertex3(0, 25, -50);

            GL.Vertex3(0, 25, -50);
            GL.Vertex3(25, 25, -50);

            GL.Vertex3(25, 25, -50);
            GL.Vertex3(25, 25, -25);

            GL.Vertex3(25, 25, -25);
            GL.Vertex3(0, 25, -25);

            //Нижние линии левой колбы
            GL.Vertex3(0, 0, 50);
            GL.Vertex3(0, 0, 25);

            GL.Vertex3(0, 0, 25);
            GL.Vertex3(25, 0, 25);

            GL.Vertex3(25, 0, 25);
            GL.Vertex3(25, 0, 50);

            GL.Vertex3(25, 0, 50);
            GL.Vertex3(0, 0, 50);
            //Вертикальные линии левой колбы
            GL.Vertex3(0, 0, 50);
            GL.Vertex3(0, 25, 50);

            GL.Vertex3(0, 0, 25);
            GL.Vertex3(0, 25, 25);

            GL.Vertex3(25, 0, 50);
            GL.Vertex3(25, 25, 50);

            GL.Vertex3(25, 0, 25);
            GL.Vertex3(25, 25, 25);
            //Верхние линии левой колбы
            GL.Vertex3(0, 25, 50);
            GL.Vertex3(0, 25, 25);

            GL.Vertex3(0, 25, 25);
            GL.Vertex3(25, 25, 25);

            GL.Vertex3(25, 25, 25);
            GL.Vertex3(25, 25, 50);

            GL.Vertex3(25, 25, 50);
            GL.Vertex3(0, 25, 50);
            GL.End();
            if(left_liquid == "Вода")
            {
                GL.Color3(Color.Blue);
            }
            else
            {
                GL.Color3(Color.Silver);
            }
            GL.Begin(BeginMode.Quads);
            //левая вода
            GL.Vertex3(0, 18, 50);
            GL.Vertex3(0, 18, 25);
            GL.Vertex3(25, 18, 25);
            GL.Vertex3(25, 18, 50);
            GL.End();
            if(right_liquid == "Вода")
            {
                GL.Color3(Color.Blue);
            }
            else
            {
                GL.Color4(119, 199, 217, 0.001f);
            }
            GL.Begin(BeginMode.Quads);
            //правая вода
            GL.Vertex3(0, 18, -50);
            GL.Vertex3(0, 18, -25);
            GL.Vertex3(25, 18, -25);
            GL.Vertex3(25, 18, -50);
            GL.End();

            GL.Color3(Color.Brown);
            GL.Begin(BeginMode.Quads);
            //Левая пробка
            //Низ левой пробки
            GL.Vertex3(10, 15.5+ coord_static, 35);
            GL.Vertex3(10, 15.5 + coord_static, 40);
            GL.Vertex3(15, 15.5 + coord_static, 40);
            GL.Vertex3(15, 15.5 + coord_static, 35);
            //дальняя сторона пробки
            GL.Vertex3(10, 15.5 + coord_static, 40);
            GL.Vertex3(10, 20.5 + coord_static, 40);
            GL.Vertex3(10, 20.5 + coord_static, 35);
            GL.Vertex3(10, 15.5 + coord_static, 35);
            //Левая сторона пробки
            GL.Vertex3(10, 15.5 + coord_static, 40);
            GL.Vertex3(10, 20.5 + coord_static, 40);
            GL.Vertex3(15, 20.5 + coord_static, 40);
            GL.Vertex3(15, 15.5 + coord_static, 40);
            //Правоя сторона
            GL.Vertex3(10, 15.5 + coord_static, 35);
            GL.Vertex3(10, 20.5 + coord_static, 35);
            GL.Vertex3(15, 20.5 + coord_static, 35);
            GL.Vertex3(15, 15.5 + coord_static, 35);
            //Ближняя сторона
            GL.Vertex3(15, 15.5 + coord_static, 35);
            GL.Vertex3(15, 20.5 + coord_static, 35);
            GL.Vertex3(15, 20.5 + coord_static, 40);
            GL.Vertex3(15, 15.5 + coord_static, 40);
            //Верхняя сторона
            GL.Vertex3(10, 20.5 + coord_static, 35);
            GL.Vertex3(15, 20.5 + coord_static, 35);
            GL.Vertex3(15, 20.5 + coord_static, 40);
            GL.Vertex3(10, 20.5 + coord_static, 40);

            //Правая пробка
            //Нижняя сторона
            GL.Vertex3(10, 15.5 + coord_non_static, -35);
            GL.Vertex3(10, 15.5 + coord_non_static, -40);
            GL.Vertex3(15, 15.5 + coord_non_static, -40);
            GL.Vertex3(15, 15.5 + coord_non_static, -35);
            //Дальняя сторона
            GL.Vertex3(10, 15.5 + coord_non_static, -35);
            GL.Vertex3(10, 20.5 + coord_non_static, -35);
            GL.Vertex3(10, 20.5 + coord_non_static, -40);
            GL.Vertex3(10, 15.5 + coord_non_static, -40);
            //Левая сторона
            GL.Vertex3(10, 15.5 + coord_non_static, -35);
            GL.Vertex3(10, 20.5 + coord_non_static, -35);
            GL.Vertex3(15, 20.5 + coord_non_static, -35);
            GL.Vertex3(15, 15.5 + coord_non_static, -35);
            //Правоя сторона
            GL.Vertex3(10, 15.5 + coord_non_static, -40);
            GL.Vertex3(10, 20.5 + coord_non_static, -40);
            GL.Vertex3(15, 20.5 + coord_non_static, -40);
            GL.Vertex3(15, 15.5 + coord_non_static, -40);
            //Ближняя сторона
            GL.Vertex3(15, 15.5 + coord_non_static, -35);
            GL.Vertex3(15, 15.5 + coord_non_static, -40);
            GL.Vertex3(15, 20.5 + coord_non_static, -40);
            GL.Vertex3(15, 20.5 + coord_non_static, -35);
            //Верхняя сторона
            GL.Vertex3(10, 20.5 + coord_non_static, -35);
            GL.Vertex3(10, 20.5 + coord_non_static, -40);
            GL.Vertex3(15, 20.5 + coord_non_static, -40);
            GL.Vertex3(15, 20.5 + coord_non_static, -35);
            GL.End();


            glControl1.SwapBuffers();
        }
        #endregion
    }
}
