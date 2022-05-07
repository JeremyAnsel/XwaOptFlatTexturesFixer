using JeremyAnsel.Xwa.Opt;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace XwaOptFlatTexturesFixer
{
    class Program
    {
        static string processName;

        [STAThread]
        static void Main()
        {
            using (var process = System.Diagnostics.Process.GetCurrentProcess())
            {
                processName = process.ProcessName;
            }

            try
            {
                Console.WriteLine("XwaOptFlatTexturesFixer");

                string openFileName = GetOpenFile();
                if (string.IsNullOrEmpty(openFileName))
                {
                    Console.WriteLine("Cancelled");
                    return;
                }

                Console.WriteLine("Opening " + openFileName + " ...");
                OptFile optFile = OptFile.FromFile(openFileName);
                optFile.CompactBuffers();
                Console.WriteLine("Opened");

                RemoveFlatTextures(optFile);

                string saveFileName = GetSaveAsFile(openFileName);
                if (string.IsNullOrEmpty(saveFileName))
                {
                    Console.WriteLine("Cancelled");
                    return;
                }

                Console.WriteLine("Saving " + saveFileName + " ...");
                optFile.Save(saveFileName);
                Console.WriteLine("Saved");

                Console.WriteLine("END");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), processName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        static string GetOpenFile()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".opt",
                CheckFileExists = true,
                Filter = "OPT files (*.opt)|*.opt"
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        static string GetSaveAsFile(string fileName)
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".opt",
                Filter = "OPT files (*.opt)|*.opt",
                FileName = Path.GetFileName(fileName)
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        static void RemoveFlatTextures(OptFile file)
        {
            Console.WriteLine("Remove Flat Textures...");

            for (int meshIndex = 0; meshIndex < file.Meshes.Count; meshIndex++)
            {
                var mesh = file.Meshes[meshIndex];

                for (int lodIndex = 0; lodIndex < mesh.Lods.Count; lodIndex++)
                {
                    var lod = mesh.Lods[lodIndex];

                    for (int faceGroupIndex = 0; faceGroupIndex < lod.FaceGroups.Count; faceGroupIndex++)
                    {
                        var faceGroup = lod.FaceGroups[faceGroupIndex];

                        for (int faceIndex = faceGroup.Faces.Count - 1; faceIndex >= 0; faceIndex--)
                        {
                            var face = faceGroup.Faces[faceIndex];

                            if (HasFaceFlatTexture(face))
                            {
                                Console.WriteLine("Face M={0} L={1} G={2} F={3} has flat texture.", meshIndex, lodIndex, faceGroupIndex, faceIndex);
                                faceGroup.Faces.RemoveAt(faceIndex);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Removed");
        }

        static bool HasFaceFlatTexture(Face face)
        {
            Indices tex = face.TextureCoordinatesIndex;

            if (tex.A == tex.B || tex.B == tex.C || tex.A == tex.C)
            {
                return true;
            }

            if (tex.D >= 0)
            {
                if (tex.A == tex.D || tex.B == tex.D || tex.C == tex.D)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
