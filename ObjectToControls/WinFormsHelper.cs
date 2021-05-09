using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

namespace ObjectToControls
{
    public static class WinFormsHelper
    {
        static Dictionary<string, Type[]> TypeByName;

        static WinFormsHelper()
        {
            TypeByName = new Dictionary<string, Type[]>();
            TypeByName["string"] = new Type[] { typeof(string) };
            TypeByName["char"] = new Type[] { typeof(char) };
            TypeByName["int"] = new Type[] { typeof(int) };
            TypeByName["bool"] = new Type[] { typeof(bool) };
            TypeByName["number"] = new Type[] { typeof(int), typeof(double), typeof(long), typeof(short), typeof(decimal) };
            TypeByName["primitive"] = new Type[] { typeof(bool), typeof(char), typeof(string), typeof(int), typeof(double), typeof(long), typeof(short), typeof(decimal) };
        }

        public static Panel ObjectToControls(object obj, Point top_left, Size size, int margin)
        {
            Panel result = new Panel();
            result.AutoScroll = true;

            var type = obj.GetType();

            foreach (PropertyInfo field in type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                

                if (TypeByName["string"].Contains(field.PropertyType))
                {
                    Action<Label, TextBox> act = (label, textbox) =>
                    {

                        label.Text = field.Name;
                        label.Name = field.Name + "_" + label.GetType();

                        textbox.Enabled = field.SetMethod == null ? false : field.SetMethod.IsPublic;
                        Binding binding = new Binding("Text", obj, field.Name, true, DataSourceUpdateMode.OnPropertyChanged);
                        binding.FormattingEnabled = true;
                        textbox.DataBindings.Add(binding);

                        textbox.Name = field.Name + "_" + textbox.GetType();

                    };
                    var temp = CreateTuple<Label, TextBox>(top_left, size, margin, act);
                    result.Controls.Add(temp.Item1);
                    result.Controls.Add(temp.Item2);
                }

                else if (TypeByName["number"].Contains(field.PropertyType))
                {
                    Action<Label, NumericUpDown> act = (label, numeric) =>
                    {
                        label.Name = field.Name + "_" + label.GetType();
                        label.Text = field.Name;

                        numeric.Name = field.Name + "_" + numeric.GetType();
                        numeric.Enabled = field.SetMethod==null ? false: field.SetMethod.IsPublic ;

                        var temp_ = field.GetValue(obj).ToString();

                        if (temp_.Contains('.'))
                            numeric.DecimalPlaces = temp_.Length - temp_.IndexOf('.') - 1;
                        if (temp_.Contains(','))
                            numeric.DecimalPlaces = temp_.Length - temp_.IndexOf(',') - 1;




                        Binding binding = new Binding("Value", obj, field.Name, true, DataSourceUpdateMode.OnPropertyChanged);
                        binding.FormattingEnabled = true;
                        numeric.DataBindings.Add(binding);

                    };
                    var temp = CreateTuple<Label, NumericUpDown>(top_left, size, margin, act);
                    result.Controls.Add(temp.Item1);
                    result.Controls.Add(temp.Item2);
                }

                else if (TypeByName["bool"].Contains(field.PropertyType))
                {
                    Action<Label, CheckBox> act = (label, checkBox) =>
                    {
                        label.Name = field.Name + "_" + label.GetType();
                        label.Text = field.Name;

                        checkBox.Name = field.Name + "_" + checkBox.GetType();
                        checkBox.Enabled = field.SetMethod == null ? false : field.SetMethod.IsPublic;

                        Binding binding = new Binding("Checked", obj, field.Name, true, DataSourceUpdateMode.OnPropertyChanged);
                        binding.FormattingEnabled = true;
                        checkBox.DataBindings.Add(binding);

                    };
                    var temp = CreateTuple<Label, CheckBox>(top_left, size, margin, act);
                    result.Controls.Add(temp.Item1);
                    result.Controls.Add(temp.Item2);
                }

                else if (typeof(IEnumerable).IsAssignableFrom(field.PropertyType))
                {
                    

                    GroupBox pa = new GroupBox();
                    pa.Text = field.Name;
                    pa.Location = top_left;
                    pa.AutoSize = true;
                    pa.Enabled = false;
                    var arr = ((IEnumerable)field.GetValue(obj)).GetEnumerator();
                    var i = 0;
                    while (arr.MoveNext())
                    {
                        var item = arr.Current;
                        if (TypeByName["primitive"].Contains(item.GetType()))
                        {

                            Action<Label, TextBox> act = (label, textBox) =>
                            {
                                label.Name = field.Name + "_" + label.GetType();
                                label.Text = i++.ToString();

                                textBox.Name = field.Name + "_" + textBox.GetType();
                                textBox.Enabled = false;
                                textBox.Text = item.ToString();

                                //MessageBox.Show(label.Location.ToString());
                                //MessageBox.Show(label.Size.ToString());

                            };
                            var temp = CreateTuple<Label, TextBox>(top_left, size, margin, act);
                            pa.Controls.Add(temp.Item1);
                            pa.Controls.Add(temp.Item2);
                        }

                        top_left = new Point(top_left.X, top_left.Y + size.Height + margin);
                    }

                    MessageBox.Show(pa.Controls.Count.ToString());
                    result.Controls.Add(pa);
                }

                else if (!TypeByName["primitive"].Contains(field.PropertyType))
                {
                    var value = field.GetValue(obj);

                    var recurs = ObjectToControls(value, top_left, size, margin);
                }



                top_left = new Point(top_left.X, top_left.Y + size.Height + margin);
            }

            return result;
        }



        public static (T, T1) CreateTuple<T, T1>(Point top_left, Size size, int margin, Action<T, T1> act = null) where T : Control, new() where T1 : Control, new()
        {

            var first = new T();
            var second = new T1();

            first.Location = top_left;
            first.Size = size;

            second.Location = new Point(top_left.X + size.Width + margin, top_left.Y);
            second.Size = size;

            act?.Invoke(first, second);

            return (first, second);
        }

    }
}



