using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace Notedown
{
    public class Note
    {
        public string Name;
        public string Text;
        public string Dir;
        
        public Note(string name, string text, string path)
        {
            Name = name;
            Text = text;
            Dir = path;
        }
        
        public void Delete()
        {
            File.Delete(Dir);
        }
        
        public void Save()
        {
            if (File.ReadAllText(Dir, Encoding.UTF8) != Text)
            {
                File.WriteAllText(Dir, Text, Encoding.UTF8);
            }
        }
        
        public bool Rename(string name)
        {
            if (!String.Equals(Name, name))
            {
                name = name.Replace(Path.DirectorySeparatorChar.ToString(), String.Empty);
                int i = Dir.LastIndexOf(Name);
                string dir = Dir.Remove(i, Name.Length).Insert(i, name);
                
                if (!File.Exists(dir))
                {
                    File.Delete(Dir);
                    Name = name;
                    Dir = dir;
                    File.WriteAllText(Dir, Text, Encoding.UTF8);
                    return true;
                }
            }
            return false;
        }
        
        public override string ToString()
        {
            return Name;
        }
    }
    
    public class NoteView
    {
        private string Dir;
        private List<Note> Notes = new List<Note>();
        public Eto.Forms.ListBox ListBox = new Eto.Forms.ListBox();
        public Eto.Forms.TextArea TextArea = new Eto.Forms.TextArea();
        
        public NoteView(string dir)
        {
            Dir = dir;
            
            TextArea.TextChanged += delegate
            {
                Notes[ListBox.SelectedIndex].Text = TextArea.Text;
            };
            
            ListBox.SelectedIndexChanged += delegate
            {
                if (ListBox.SelectedIndex < 0) return;
                TextArea.Text = Notes[ListBox.SelectedIndex].Text;
            };
        }
        
        public Note this[int i]
        {
            get
            {
                return Notes[i];
            }
            set
            {
                Notes[i] = value;
            }
        }
        
        public Note CurrentNote
        {
            get { return ListBox.SelectedIndex >= 0 ? Notes[ListBox.SelectedIndex] : null; }
        }
        
        public void AddNote(string name) { AddNote(name, string.Empty, Dir + name + ".txt"); }
        public void AddNote(string name, string text, string file)
        {
            Note note = new Note(name, text, file);
            Notes.Add(note);
            ListBox.Items.Add(note);
            
            if (!File.Exists(file))
            {
                if (!Directory.Exists(Dir))
                {
                    Directory.CreateDirectory(Dir);
                }
                File.WriteAllText(file, text, Encoding.UTF8);
            }
        }
        
        public void DeleteNote() { DeleteNote(ListBox.SelectedIndex); }
        public void DeleteNote(Note note) { DeleteNote(ListBox.Items.IndexOf(note)); }
        public void DeleteNote(int index)
        {
            Notes[index].Delete();
            Notes.RemoveAt(index);
            ListBox.Items.RemoveAt(index);
        }
        
        public void Save()
        {
            foreach (Note note in Notes)
            {
                note.Save();
            }
        }
        
        public static NoteView CreateFromDirectory(string dir)
        {
            NoteView result = new NoteView(dir);
            
            if (!Directory.Exists(dir)) return result;
            
            string[] files = Directory.GetFiles(dir);
            if (files.Length != 0)
            {
                foreach (string file in files)
                {
                    string name = file.Replace(dir, "").Replace(".txt", "");
                    string text = File.ReadAllText(file, Encoding.UTF8);
                    result.AddNote(name, text, file);
                }
                result.ListBox.SelectedIndex = 0;
            }
            
            return result;
        }
    }
}