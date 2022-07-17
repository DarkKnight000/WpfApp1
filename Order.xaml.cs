using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для Order.xaml
    /// </summary>
    public partial class Order : Window
    {
        public Order()
        {
            InitializeComponent();
        }

        DataTable dt_clients;
        int client_id;
        public int order_id;
        double discount = 0;
        double discount_cost = 0.00;
        DataBase dataBase = new DataBase();

        // Вывод клиентов в комбобокс:
        public void show_clients()
        {
            DataBase.sqlcmd = "SELECT * FROM clients ORDER BY client_id";
            dt_clients = dataBase.Connect(DataBase.sqlcmd);

            try
            {
                for (int i = 0; i < dt_clients.Rows.Count; i++)
                {
                    comboBox.Items.Add($"{dt_clients.Rows[i][1]} \n{dt_clients.Rows[i][2]}");
                }
            }
            catch { }
        }

        // Заполнение данными выбранного клиента:
        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                string date = dt_clients.Rows[comboBox.SelectedIndex][4].ToString();
                date = date.Remove(date.Length - 13);

                label_FIO.Content = dt_clients.Rows[comboBox.SelectedIndex][1];
                label_Phone.Content = dt_clients.Rows[comboBox.SelectedIndex][2];
                label_Email.Content = dt_clients.Rows[comboBox.SelectedIndex][3];
                client_id = int.Parse(dt_clients.Rows[comboBox.SelectedIndex][0].ToString());

                string dateNow = DateTime.Now.ToString();
                dateNow = dateNow.Remove(dateNow.Length - 14);

                //MessageBox.Show(date + "   " + dateNow);
                if (date == dateNow)
                {
                    discount = 5;
                }
                else
                {
                    discount = 0;
                }
                label_discount.Content = $"{discount}%";

                discount_cost = MainWindow.costcount * (1 - discount / 100);
                discount_cost = Math.Round(discount_cost, 2);
                //MessageBox.Show(discount_cost.ToString());
                label_totalCost.Content = discount_cost;
            }
            catch { }

        }

        // Заполнение выбранными товарами из корзины:
        public void add_cart()
        {
            DataTable dt_fromCart = new DataTable();
            dt_fromCart.Columns.Add("Наименование", typeof(string));
            dt_fromCart.Columns.Add("Количество", typeof(int));
            dt_fromCart.Columns.Add("Цена за штуку", typeof(double));
            dt_fromCart.Columns.Add("Общая стоимость", typeof(double));

            if (MainWindow.show_order == false)
            {
                show_clients();
                for (int i = 0; i < MainWindow.dt_cart.Rows.Count; i++)
                {
                    dt_fromCart.Rows.Add(MainWindow.dt_cart.Rows[i][1], MainWindow.dt_cart.Rows[i][2], MainWindow.dt_cart.Rows[i][5], MainWindow.dt_cart.Rows[i][3]);
                }
                label_totalCost.Content = MainWindow.costcount;
            }
            else
            {
                for (int i = 0; i < MainWindow.dt_order.Rows.Count; i++)
                {
                    dt_fromCart.Rows.Add(MainWindow.dt_order.Rows[i][1], MainWindow.dt_order.Rows[i][2], MainWindow.dt_order.Rows[i][3], double.Parse(MainWindow.dt_order.Rows[i][2].ToString()) * double.Parse(MainWindow.dt_order.Rows[i][3].ToString()));
                }
                MainWindow.show_order = false;
            }

            dataGrid.ItemsSource = dt_fromCart.DefaultView;
        }

        // Кнопка Оформить заказ:
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedItem == null || label_FIO.Content == null)
            {
                MessageBox.Show("Выберите клиента!");
            }
            else
            {
                string discount_cost1 = discount_cost.ToString();
                discount_cost1 = discount_cost1.Replace(",", ".");
                DataBase.sqlcmd = $@"START TRANSACTION;

                                     INSERT INTO `order`
                                     (client_id, Дата, `Скидка`, `Общая сумма`, `Статус заказа`)
                                     VALUES ({client_id}, NOW(), '{discount}', '{discount_cost1}', 4);

                                     SELECT order_id 
                                     FROM `order` 
                                     ORDER BY order_id DESC 
                                     LIMIT 1;

                                     COMMIT;"
                ;

                DataTable dt = dataBase.Connect(DataBase.sqlcmd);

                DataBase.sqlcmd = $@"START TRANSACTION;

                                     INSERT INTO cart
                                     (product_id, Количество, order_id)
                                     VALUES "
                ;

                for (int i = 0; i < MainWindow.dt_cart.Rows.Count; i++)
                {
                    DataBase.sqlcmd += $@"({MainWindow.dt_cart.Rows[i][0]}, {MainWindow.dt_cart.Rows[i][2]}, {dt.Rows[0][0]}), ";
                }
                DataBase.sqlcmd = DataBase.sqlcmd.Remove(DataBase.sqlcmd.Length - 2);
                DataBase.sqlcmd += ";\n";

                for (int i = 0; i < MainWindow.dt_cart.Rows.Count; i++)
                {
                    DataBase.sqlcmd += $@"UPDATE product
                                          SET `На складе` = `На складе` - {MainWindow.dt_cart.Rows[i][2]}
                                          WHERE product_id = {MainWindow.dt_cart.Rows[i][0]};" + "\n"
                    ;
                }
                DataBase.sqlcmd += "COMMIT;";

                dataBase.Connect(DataBase.sqlcmd);

                MessageBox.Show("Заказ принят!");
                button1_Click(null, null);
            }
        }

        // Кнопка Назад:
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Выбор статуса заказа:
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            label_OrderStatus.Content = selectedItem.Content.ToString();

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(order_id.ToString());
            //MessageBox.Show(label_OrderStatus.Content.ToString());
            string status = label_OrderStatus.Content.ToString();
            DataBase.sqlcmd = $@"UPDATE `order`, `order_status`
                                 SET `order`.`Статус заказа` = `order_status`.`status_id` 
                                 WHERE `order_status`.`Статус заказа` = '{status}' AND `order`.`order_id` = {order_id}"
            ;
            dataBase.Connect(DataBase.sqlcmd);
        }
    }
}
