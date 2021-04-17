using System.IO;
using System.Diagnostics;

namespace Translator
{
    public static class PythonTranslator
    {
        // relative path (from Assets) to the CSharp Translator program
        public const string PROGRAM_REL_PATH = @"Assets\Translators\PythonTranslator\bin\dist\translator.exe";

        public static string translatePython(string input_file, string output_file)
        {
            // check the pathes
            if (!File.Exists(input_file))
            {
                return "FileNotFound (1): " + input_file;
            }

            // Create Process
            Process p_process = new Process();
            p_process.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), PROGRAM_REL_PATH);
            if (!File.Exists(p_process.StartInfo.FileName))
            {
                return "Python Translator binaries not found: " + p_process.StartInfo.FileName;
            }
            p_process.StartInfo.Arguments = $" -i \"{input_file}\" -o \"{output_file}\"";
            p_process.StartInfo.UseShellExecute = false;
            p_process.StartInfo.CreateNoWindow = true; // no window
            p_process.StartInfo.RedirectStandardOutput = true; // redirect stdout so we can read it later
            p_process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            // Run Process
            p_process.Start();
            string output = p_process.StandardOutput.ReadToEnd();
            p_process.WaitForExit();

            // Log the output
            StreamWriter log_file = new StreamWriter("py translator log.log");
            log_file.Write(output);
            log_file.Close();

            // return the output
            return output;
        }
    }
}