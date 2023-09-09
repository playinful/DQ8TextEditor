using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DQ8TextEditor
{
    public class MessageFile
    {
        public byte[] Header { get; set; }
        public int[] Pointers { get; set; }
        public List<string> Strings { get; set; }
        public bool Changed { get; set; } = false;
        public string Source { get; set; }

        public bool Ready()
        {
            return Pointers != null && Strings != null && Pointers.Length > 0 && Strings.Count > 0;
        }
        public void Load(string address)
        {
            if (File.Exists(address))
            {
                bool succ = Close();

                if (succ)
                {
                    Source = address;

                    byte[] file_data = File.ReadAllBytes(address);
                    // string file_text = File.ReadAllText(address);

                    int last_end_index = -1;
                    int penultimate_end_index = -1;
                    for (int i = file_data.Length - 5; i >= 0; i--)
                    {
                        if (last_end_index < 0 && file_data[i].Equals(91) && file_data[i + 1].Equals(101) && file_data[i + 2].Equals(110) && file_data[i + 3].Equals(100) && file_data[i + 4].Equals(93))
                        {
                            last_end_index = i;
                            continue;
                        }
                        if (last_end_index >= 0 && file_data[i].Equals(91) && file_data[i + 1].Equals(101) && file_data[i + 2].Equals(110) && file_data[i + 3].Equals(100) && file_data[i + 4].Equals(93))
                        {
                            penultimate_end_index = i;
                            break;
                        }
                    }

                    if (last_end_index > -1 && penultimate_end_index <= -1)
                    {
                        Header = new byte[0x10];
                        for (int i = 0; i < Header.Length; i++)
                        {
                            Header[i] = file_data[i];
                        }

                        Pointers = new int[1] { 0x14 };

                        Strings = new List<string>();
                        foreach (int ptr in Pointers)
                        {
                            string str = Encoding.UTF8.GetString(file_data, ptr, file_data.Length - ptr);
                            str = new Regex("[\\S\\s]*?(?=\\[end\\])").Match(str).Groups[0].Value;
                            str = str.Replace("\n", "\r\n");
                            Strings.Add(str);
                        }

                        return;
                    }

                    if (last_end_index <= -1 && penultimate_end_index <= -1)
                    {
                        MessageBox.Show("Not a valid Dragon Quest VIII text file.");
                        return;
                    }

                    int last_string_index = penultimate_end_index + 5;

                    int last_pointer_index = -1;
                    for (int i = file_data.Length - (file_data.Length % 4); i >= 0; i -= 4)
                    {
                        if (i + 4 >= file_data.Length)
                            continue;

                        byte[] bts;

                        if (BitConverter.IsLittleEndian)
                            bts = new byte[4] { file_data[i], file_data[i + 1], file_data[i + 2], file_data[i + 3] };
                        else
                            bts = new byte[4] { file_data[i + 3], file_data[i + 2], file_data[i + 1], file_data[i] };
                        int frombytes = BitConverter.ToInt32(bts, 0);

                        if (frombytes == last_string_index)
                        {
                            last_pointer_index = i;
                        }
                    }

                    if (last_pointer_index <= -1)
                    {
                        MessageBox.Show("Not a valid Dragon Quest VIII text file.");
                        return;
                    }

                    int first_string_index = last_pointer_index + 4;

                    int first_pointer_index = -1;
                    for (int i = last_pointer_index; i >= 0; i -= 4)
                    {
                        if (i + 4 >= file_data.Length)
                            continue;

                        byte[] bts;

                        if (BitConverter.IsLittleEndian)
                            bts = new byte[4] { file_data[i], file_data[i + 1], file_data[i + 2], file_data[i + 3] };
                        else
                            bts = new byte[4] { file_data[i + 3], file_data[i + 2], file_data[i + 1], file_data[i] };
                        int frombytes = BitConverter.ToInt32(bts, 0);

                        if (frombytes == first_string_index)
                        {
                            first_pointer_index = i;
                        }
                    }

                    Header = new byte[first_pointer_index];
                    for (int i = 0; i < Header.Length; i++)
                    {
                        Header[i] = file_data[i];
                    }

                    Pointers = new int[(first_string_index - first_pointer_index) / 4];
                    for (int i = 0; i < Pointers.Length; i++)
                    {
                        byte[] bts;
                        int ind = Header.Length + (i * 4);

                        if (BitConverter.IsLittleEndian)
                            bts = new byte[4] { file_data[ind], file_data[ind + 1], file_data[ind + 2], file_data[ind + 3] };
                        else
                            bts = new byte[4] { file_data[ind + 3], file_data[ind + 2], file_data[ind + 1], file_data[ind] };

                        Pointers[i] = BitConverter.ToInt32(bts, 0);
                    }

                    Strings = new List<string>();
                    foreach (int ptr in Pointers)
                    {
                        string str = Encoding.UTF8.GetString(file_data, ptr, file_data.Length - ptr);
                        str = new Regex("[\\S\\s]*?(?=\\[end\\])").Match(str).Groups[0].Value;
                        str = str.Replace("\n", "\r\n");
                        Strings.Add(str);
                    }

                    /*//int first_end_index = -1;
                    //for (int i = 0; i < file_data.Length; i++)
                    //{
                    //    if (file_data[i].Equals(91) && file_data[i + 1].Equals(101) && file_data[i + 2].Equals(110) && file_data[i + 3].Equals(100) && file_data[i + 4].Equals(93))
                    //    {
                    //        first_end_index = i;
                    //        break;
                    //    }
                    //}
                    //
                    //if (first_end_index > -1)
                    //{
                    //    // find the second index
                    //    int second_pointer_index = -1;
                    //    for (int i = 0; i < file_data.Length - 4; i += 4)
                    //    {
                    //        byte[] bytes = new byte[4];
                    //        Array.Copy(file_data, i, bytes, 0, 4);
                    //        int j = BitConverter.ToInt32(bytes, 0);
                    //
                    //        if (j == first_end_index + 5)
                    //        {
                    //            second_pointer_index = i;
                    //            break;
                    //        }
                    //    }
                    //
                    //    if (second_pointer_index > -1)
                    //    {
                    //        int first_pointer_index = second_pointer_index - 4;
                    //        Header = new byte[first_pointer_index];
                    //        Array.Copy(file_data, 0, Header, 0, first_pointer_index);
                    //
                    //        byte[] fpi = new byte[4];
                    //        Array.Copy(file_data, first_pointer_index, fpi, 0, 4);
                    //
                    //        int first_string_index = BitConverter.ToInt32(fpi, 0);
                    //        Pointers = new int[(first_string_index - first_pointer_index) / 4];
                    //        for (int i = 0; i < Pointers.Length; i++)
                    //        {
                    //            byte[] bytes = new byte[4];
                    //            Array.Copy(file_data, first_pointer_index + (i * 4), bytes, 0, 4);
                    //            Pointers[i] = BitConverter.ToInt32(bytes, 0);
                    //        }
                    //        Strings = new List<string>();
                    //        for (int i = 0; i < Pointers.Length; i++)
                    //        {
                    //            byte[] bytes;
                    //            int len;
                    //            if (i < Pointers.Length - 1)
                    //            {
                    //                len = Pointers[i + 1] - Pointers[i];
                    //                bytes = new byte[len];
                    //            }
                    //            else
                    //            {
                    //                len = file_data.Length - Pointers[i];
                    //                bytes = new byte[len];
                    //            }
                    //            Array.Copy(file_data, Pointers[i], bytes, 0, len);
                    //
                    //            string str = System.Text.Encoding.Default.GetString(bytes);
                    //            str = str.Substring(0, str.Length - 5);
                    //
                    //            Strings.Add(str);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        MessageBox.Show("Not a valid Dragon Quest VIII text file.");
                    //    }
                    //}
                    //else
                    //{
                    //    MessageBox.Show("Not a valid Dragon Quest VIII text file.");
                    //}*/

                }
            }
            else if (address != "")
            {
                MessageBox.Show("No such file exists.");
            }
        }
        public void ShowDetails(ListBox lb, TextBox tb)
        {
            if (Ready())
            {
                lb.Items.Clear();
                for (int i = 0; i < Pointers.Length; i++)
                {
                    lb.Items.Add(i.ToString());
                }
                lb.SelectedIndex = 0;
                if (Pointers.Length > 0)
                {
                    tb.Enabled = true;
                    tb.Text = Strings[lb.SelectedIndex];
                }
            }
        }
        public bool Close()
        {
            bool allow_close = false;

            if (Changed)
            {
                var result = MessageBox.Show("You have unsaved changes. Would you like to save?", "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                switch (result)
                {
                    case DialogResult.Cancel:
                        allow_close = false;
                        break;
                    case DialogResult.Yes:
                        allow_close = true;
                        break;
                    case DialogResult.No:
                        allow_close = true;
                        break;
                }
            }
            else
                allow_close = true;

            if (allow_close)
            {
                Header = null;
                Pointers = null;
                Strings = null;
                Changed = false;
            }

            return allow_close;
        }
        public void EditString(int id, string text)
        {
            if (Strings != null && Strings[id] != text)
            {
                Strings[id] = text;
                Changed = true;
            }
        }
        public void Save(string address = ":")
        {
            if (address == ":")
                address = Source;

            if (address != null && address != "" && Ready())
            {
                Pointers = new int[Strings.Count];

                for (int i = 0; i < Strings.Count; i++)
                {
                    Strings[i] = Strings[i].Replace("\r\n", "\n") + "[end]";
                }

                int first_str_index = Header.Length + (Pointers.Length * 4);

                int str_index = first_str_index;
                for (int i = 0; i < Pointers.Length; i++)
                {
                    Pointers[i] = str_index;
                    str_index += Encoding.UTF8.GetBytes(Strings[i]).Length;
                }

                List<byte> write_bytes = new List<byte>(Header);
                foreach (int pointer in Pointers)
                {
                    write_bytes.AddRange(BitConverter.GetBytes(pointer));
                }

                foreach (string str in Strings)
                {
                    write_bytes.AddRange(Encoding.UTF8.GetBytes(str));
                    //                    foreach (char c in str)
                    //                    {
                    //                        write_bytes.Add((byte)c);
                    //                    }
                }

                byte[] byte_arr = write_bytes.ToArray();

                File.WriteAllBytes(address, byte_arr);

                for (int i = 0; i < Strings.Count; i++)
                {
                    Strings[i] = Strings[i].Substring(0, Strings[i].Length - 5);
                }

                Changed = false;
            }
        }
    }
}