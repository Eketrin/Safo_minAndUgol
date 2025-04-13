using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Ygl.xaml
    /// </summary>
    public partial class Ygl : Page
    {
        public Ygl()
        {
            InitializeComponent();
        }
        private void ClearCostMatrix_Click(object sender, RoutedEventArgs e)
        {
            SupplyTextBox.Clear();
            DemandTextBox.Clear();
            CostTextBox.Clear();
            ResultDataGrid.ItemsSource = null;
            CostTextBlock.Text = string.Empty;
        }
        private void OnBuildPlanClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Парсим данные из текстовых полей
                var supply = ParseInput(SupplyTextBox.Text);
                var demand = ParseInput(DemandTextBox.Text);
                var cost = ParseCostMatrix(CostTextBox.Text);

                // Проверка на балансировку задачи
                int totalSupply = supply.Sum();
                int totalDemand = demand.Sum();

                if (totalSupply != totalDemand)
                {
                    if (totalSupply > totalDemand)
                    {
                        Array.Resize(ref demand, demand.Length + 1);
                        demand[demand.Length - 1] = totalSupply - totalDemand;
                        cost = ResizeCostMatrix(cost, cost.GetLength(0), cost.GetLength(1) + 1);
                    }
                    else
                    {
                        Array.Resize(ref supply, supply.Length + 1);
                        supply[supply.Length - 1] = totalDemand - totalSupply;
                        cost = ResizeCostMatrix(cost, cost.GetLength(0) + 1, cost.GetLength(1));
                    }
                }

                if (supply.Length != cost.GetLength(0) || demand.Length != cost.GetLength(1))
                {
                    MessageBox.Show("Размерность предложений и потребностей не соответствует размерности стоимости.");
                    return;
                }

                var result = GetNorthWestCornerPlan(supply, demand, cost);
                var totalCost = CalculateTotalCost(result, cost);

                // Обновляем TextBlock для отображения общей стоимости
                CostTextBlock.Text = $"Общая стоимость F = {totalCost}";

                var resultList = new List<ResultRow>();
                for (int i = 0; i < result.Count; i++)
                {
                    var row = new ResultRow
                    {
                        Quantities = string.Join(", ", result[i])
                    };
                    resultList.Add(row);
                }

                ResultDataGrid.ItemsSource = resultList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private int[] ParseInput(string input)
        {
            return input.Split(',').Select(int.Parse).ToArray();
        }

        private int[,] ParseCostMatrix(string input)
        {
            var rows = input.Split(';');
            var matrix = new int[rows.Length, rows[0].Split(',').Length];

            for (int i = 0; i < rows.Length; i++)
            {
                var cols = rows[i].Split(',');
                for (int j = 0; j < cols.Length; j++)
                {
                    matrix[i, j] = int.Parse(cols[j]);
                }
            }

            return matrix;
        }

        // Метод для изменения размера матрицы стоимости (добавление фиктивных строк или столбцов)
        private int[,] ResizeCostMatrix(int[,] cost, int newRowCount, int newColCount)
        {
            int[,] resizedMatrix = new int[newRowCount, newColCount];


            for (int i = 0; i < cost.GetLength(0); i++)
            {
                for (int j = 0; j < cost.GetLength(1); j++)
                {
                    resizedMatrix[i, j] = cost[i, j];
                }
            }

            for (int i = 0; i < resizedMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < resizedMatrix.GetLength(1); j++)
                {
                    if (i >= cost.GetLength(0) || j >= cost.GetLength(1))
                    {
                        resizedMatrix[i, j] = 0;
                    }
                }
            }

            return resizedMatrix;
        }

        // Алгоритм метода северо-западного угла
        private List<int[]> GetNorthWestCornerPlan(int[] supply, int[] demand, int[,] cost)
        {
            int m = supply.Length;
            int n = demand.Length;
            var plan = new List<int[]>(m);

            for (int i = 0; i < m; i++)
            {
                var row = new int[n];
                plan.Add(row);
            }

            int iSupply = 0, jDemand = 0;

            while (iSupply < m && jDemand < n)
            {
                int x = Math.Min(supply[iSupply], demand[jDemand]);
                plan[iSupply][jDemand] = x;

                supply[iSupply] -= x;
                demand[jDemand] -= x;

                if (supply[iSupply] == 0) iSupply++;
                if (demand[jDemand] == 0) jDemand++;
            }

            return plan;
        }

        // Метод для подсчета общей стоимости по опорному плану
        private int CalculateTotalCost(List<int[]> plan, int[,] cost)
        {
            int totalCost = 0;

            for (int i = 0; i < plan.Count; i++)
            {
                for (int j = 0; j < plan[i].Length; j++)
                {
                    totalCost += plan[i][j] * cost[i, j];
                }
            }

            return totalCost;
        }

        public class ResultRow
        {
            public string Quantities { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}