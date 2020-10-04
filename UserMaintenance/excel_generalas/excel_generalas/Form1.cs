﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace excel_generalas
{
    public partial class Form1 : Form
    {
        List<Flat> flats;

        RealEstateEntities context = new RealEstateEntities();

        Excel.Application xlApp;
        Excel.Workbook xlWB;
        Excel.Worksheet xlSheet;

        public Form1()
        {
            InitializeComponent();
            LoadData();
            CreateExcel();
        }

        private void CreateExcel()
        {
            try
            {
                xlApp = new Excel.Application();

                xlWB = xlApp.Workbooks.Add(Missing.Value);

                xlSheet = xlWB.ActiveSheet;

                CreateTable();

                xlApp.Visible = true;
                xlApp.UserControl = true;
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Error: {0}\nLine: {1}", ex.Message, ex.Source);
                MessageBox.Show(errMsg, "Error");

                xlWB.Close(false, Type.Missing, Type.Missing);
                xlApp.Quit();
                xlWB = null;
                xlApp = null;
            }
        }
            
        private void CreateTable()
        {
            string[] headers = new string[] {
                        "Kód",
                        "Eladó",
                        "Oldal",
                        "Kerület",
                        "Lift",
                        "Szobák száma",
                        "Alapterület (m2)",
                        "Ár (mFt)",
                        "Négyzetméter ár (Ft/m2)"};

            for (int i = 0; i < headers.Length; i++)
            {
                xlSheet.Cells[1, i + 1] = headers[i];
            }
            object[,] values = new object[flats.Count, headers.Length];
            int counter = 0;
            foreach (Flat f in flats)
            {
                values[counter, 0] = f.Code;
                values[counter, 1] = f.Vendor;
                values[counter, 2] = f.Side;
                values[counter, 3] = f.District;
                if (f.Elevator)
                    values[counter, 4] = "van";
                else
                    values[counter, 4] = "nincs";
                values[counter, 5] = f.NumberOfRooms;
                values[counter, 6] = f.FloorArea;
                values[counter, 7] = f.Price;
                values[counter, 8] = "=" + GetCell(counter + 2, 8) + "/" + GetCell(counter + 2, 7);
                counter++;
            }
            xlSheet.get_Range(
                GetCell(2, 1),
                GetCell(1 + values.GetLength(0), values.GetLength(1))).Value2 = values;

            FormatTable();
        }

        private void FormatTable()
        {
            int lastRowID = xlSheet.UsedRange.Rows.Count;
            int lastCoolumnID = xlSheet.UsedRange.Columns.Count;

            Excel.Range headerRange = xlSheet.get_Range(GetCell(1, 1), GetCell(1, lastCoolumnID));
            headerRange.Font.Bold = true;
            headerRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            headerRange.EntireColumn.AutoFit();
            headerRange.RowHeight = 40;
            headerRange.Interior.Color = Color.LightBlue;
            headerRange.BorderAround2(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlThick);


            Excel.Range tableRange = xlSheet.get_Range(GetCell(1, 1), GetCell(lastRowID, lastCoolumnID));
            tableRange.BorderAround2(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlThick);

            Excel.Range firstColumn = xlSheet.get_Range(GetCell(2, 1), GetCell(lastRowID,1));
            firstColumn.Interior.Color = Color.LightYellow;
            firstColumn.Font.Bold = true;

            Excel.Range lastColumn = xlSheet.get_Range(GetCell(2, lastCoolumnID), GetCell(lastRowID, lastCoolumnID));
            lastColumn.Interior.Color = Color.LightGreen;
            

        }

        private string GetCell(int x, int y)
        {
            string ExcelCoordinate = "";
            int dividend = y;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                ExcelCoordinate = Convert.ToChar(65 + modulo).ToString() + ExcelCoordinate;
                dividend = (int)((dividend - modulo) / 26);
            }
            ExcelCoordinate += x.ToString();

            return ExcelCoordinate;
        }
        

        private void LoadData()
        {
            flats = context.Flats.ToList();
        }
    }
}
