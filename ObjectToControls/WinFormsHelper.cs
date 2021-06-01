using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;

namespace ObjectToControls
{
    //Created by Konstantin Bobrovskii
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

        public static Panel ObjectToControls(object obj, Size size, int margin)
        {
            Point top_left = new Point(5, 5);

            Panel result = new Panel();
            result.AutoScroll = true;

            var type = obj.GetType();

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var primitives_count = properties.Where((property)=>TypeByName["primitive"].Contains(property.PropertyType)).Count();
            var arrays_count = properties.Where((property) => typeof(IEnumerable).IsAssignableFrom(property.PropertyType)).Count();

            var elements_count = primitives_count + arrays_count;

            foreach (PropertyInfo property in properties)
            {
                

                if (TypeByName["string"].Contains(property.PropertyType))
                {
                    Action<Label, TextBox> act = (label, textbox) =>
                    {
                        label.Text = property.Name;
                        label.Name = property.Name + "_" + label.GetType();

                        textbox.Enabled = property.SetMethod == null ? false : property.SetMethod.IsPublic;
                        Binding binding = new Binding("Text", obj, property.Name, true, DataSourceUpdateMode.OnPropertyChanged);
                        binding.FormattingEnabled = true;
                        textbox.DataBindings.Add(binding);

                        textbox.Name = property.Name + "_" + textbox.GetType();

                    };
                    var temp = CreateTuple<Label, TextBox>(top_left, size, margin, act);
                    result.Controls.Add(temp.Item1);
                    result.Controls.Add(temp.Item2);
                }

                else if (TypeByName["number"].Contains(property.PropertyType))
                {
                    Action<Label, NumericUpDown> act = (label, numeric) =>
                    {
                        label.Name = property.Name + "_" + label.GetType();
                        label.Text = property.Name;

                        numeric.Name = property.Name + "_" + numeric.GetType();
                        numeric.Enabled = property.SetMethod==null ? false: property.SetMethod.IsPublic ;

                        var temp_ = property.GetValue(obj).ToString();

                        if (temp_.Contains('.'))
                            numeric.DecimalPlaces = temp_.Length - temp_.IndexOf('.') - 1;
                        if (temp_.Contains(','))
                            numeric.DecimalPlaces = temp_.Length - temp_.IndexOf(',') - 1;




                        Binding binding = new Binding("Value", obj, property.Name, true, DataSourceUpdateMode.OnPropertyChanged);
                        binding.FormattingEnabled = true;
                        numeric.DataBindings.Add(binding);

                    };
                    var temp = CreateTuple<Label, NumericUpDown>(top_left, size, margin, act);
                    result.Controls.Add(temp.Item1);
                    result.Controls.Add(temp.Item2);
                }

                else if (TypeByName["bool"].Contains(property.PropertyType))
                {
                    Action<Label, CheckBox> act = (label, checkBox) =>
                    {
                        label.Name = property.Name + "_" + label.GetType();
                        label.Text = property.Name;

                        checkBox.Name = property.Name + "_" + checkBox.GetType();
                        checkBox.Enabled = property.SetMethod == null ? false : property.SetMethod.IsPublic;

                        Binding binding = new Binding("Checked", obj, property.Name, true, DataSourceUpdateMode.OnPropertyChanged);
                        binding.FormattingEnabled = true;
                        checkBox.DataBindings.Add(binding);

                    };
                    var temp = CreateTuple<Label, CheckBox>(top_left, size, margin, act);
                    result.Controls.Add(temp.Item1);
                    result.Controls.Add(temp.Item2);
                }

                else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    const int labelHeight = 40;

                    GroupBox box = new GroupBox();
                    var pan = new Panel();
                    box.Controls.Add(pan);
                    box.Size = new Size(size.Width * 2 + margin, (labelHeight + margin) * 10);
                    box.Text = property.Name;
                    box.Location = top_left;
                   
                    pan.AutoScroll = true;
                    pan.Dock = DockStyle.Fill;

                    var localPoint = new Point(0, 0);

                    var arr = ((IEnumerable)property.GetValue(obj)).GetEnumerator();
                    var i = 0;
                    while (arr.MoveNext())
                    {
                        var item = arr.Current;
                        if (TypeByName["primitive"].Contains(item.GetType()))
                        {

                            Action<Label, TextBox> act = (label, textBox) =>
                            {
                                label.Name = property.Name + "_" + label.GetType();
                                label.Text = i++.ToString();
                                label.AutoSize = true;
                                label.Width = label.PreferredWidth;
                                textBox.Name = property.Name + "_" + textBox.GetType();
                                textBox.Enabled = false;
                                textBox.Text = item.ToString();
                                textBox.Location = new Point(label.Location.X + label.Width + 5, textBox.Location.Y);



                            };
                            var temp = CreateTuple<Label, TextBox>(localPoint, size, margin, act);
                            pan.Controls.Add(temp.Item1);
                            pan.Controls.Add(temp.Item2);

                            
                        }

                        localPoint = new Point(localPoint.X, localPoint.Y + size.Height + margin);
                    }


                    result.Controls.Add(box);

                    top_left = new Point(top_left.X, box.Location.Y + box.Height + margin);
                }

                else if (!TypeByName["primitive"].Contains(property.PropertyType))
                {
                    GroupBox box = new GroupBox();

                    
                    box.Size = new Size(size.Width * 2 + margin, (40 + margin) * 10);
                    box.Text = property.Name;
                    box.Location = top_left;



                    var value = property.GetValue(obj);

                    var recurs = ObjectToControls(value, size, margin);

                    

                    box.Controls.Add(recurs);

                    recurs.AutoScroll = true;
                    recurs.Dock = DockStyle.Fill;

                    result.Controls.Add(box);

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



