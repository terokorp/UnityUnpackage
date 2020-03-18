using System;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace UnityUnpackage
{
    public partial class MainWindow : Form
    {
        private int assetCounter;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void Form1_DragLeave(object sender, EventArgs e)
        {
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if(Path.GetExtension(files[0]).Equals(".unitypackage")) {
                Console.WriteLine("GotFileDrop" + files[0]);
                ExtractUnitypackage(files[0]);
                MessageBox.Show(assetCounter + " assets extracted");
            }
        }

        private void ExtractUnitypackage(string filename)
        {
            Console.WriteLine("ExtractUnitypackage");
            var tempFolder = Path.Combine(Path.GetTempPath(), "tmp_" + Path.GetFileNameWithoutExtension(filename));
            var targetFolder = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + "_extracted");

            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);
            if (Directory.Exists(targetFolder))
            {
                MessageBox.Show("Target folder already exists");
                return;
            }
            Directory.CreateDirectory(tempFolder);
            Directory.CreateDirectory(targetFolder);

            ExtractTGZ(filename, tempFolder);
            ProcessExtracted(tempFolder, targetFolder);

            Directory.Delete(tempFolder, true);
        }

        public void ExtractTGZ(string gzArchiveName, string destFolder)
        {
            Stream inStream = File.OpenRead(gzArchiveName);
            Stream gzipStream = new GZipInputStream(inStream);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
            tarArchive.ExtractContents(destFolder);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
        }

        private void ProcessExtracted(string tempFolder, string targetFolder)
        {
            assetCounter = 0;
            foreach (string d in Directory.EnumerateDirectories(tempFolder))
            {
                string relativePath = "";
                string targetFullPath = "";
                string targetFullFile = "";

                if (File.Exists(Path.Combine(d, "pathname")))
                {
                    relativePath = File.ReadAllText(Path.Combine(d, "pathname"));
                    targetFullPath = Path.GetDirectoryName(Path.Combine(targetFolder, relativePath));
                    targetFullFile = Path.Combine(targetFolder, relativePath);
                }

                // If asset file exists
                if (File.Exists(Path.Combine(d, "asset")))
                {
                    Directory.CreateDirectory(targetFullPath);
                    File.Move(Path.Combine(d, "asset"), targetFullFile);
                    assetCounter++;
                }

                // If asset meta file exists
                if (File.Exists(Path.Combine(d, "asset.meta")))
                {
                    Directory.CreateDirectory(targetFullPath);
                    File.Move(Path.Combine(d, "asset.meta"), targetFullFile + ".meta");
                }

                // If asset preview file exists
                if (File.Exists(Path.Combine(d, "preview.png")))
                {

                }
            }
        }
    }
}
