// Parses Amazon AWS S3 logs
// (C) 2013 zapmaker
//
// Written using Visual Studio 2005
//
// Copy your logs into a folder and run
//
// License: Apache 2.0
//
// Settings in Win7 stored C:\Users\<user>\AppData\Local\zapmaker\ParseAws.vshost.exe_Url_ax5itjxyb5r3ocqkwamw1lojboffp2mw\1.0.0.0
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Windows;
//using System.Text.RegularExpressions;
using System.Data.Odbc;


/* postgres table required
create table aws_stats
(
	BucketOwner varchar(255) not null,
	Bucket varchar(255) not null,
	Time timestamp not null,
	RemoteIP varchar(255) null,
	Requester varchar(255) null,
	RequestID varchar(255) not null primary key,
	Operation varchar(255) null,
	Key varchar(255) null,
	RequestURI varchar(255) not null,
	HTTPStatus int not null,
	ErrorCode varchar(255) null,
	BytesSent int null,
	ObjectSize int null,
	TotalTime int null,
	TurnAroundTime int null,
	Referrer varchar(1024) null,
	UserAgent varchar(1024) null,
	VersionId varchar(255) null
);
 */

namespace ParseAws
{
    public partial class AWSLogParser : Form
    {
        public const string connectStr = "DSN=PostgreSQL35W-AWS;" +
            "UID=postgres;" +
            "PWD=myhappypasswordchangeme";

        private int totalFileCount;
        private int partialGoodFileCount;
        private int fullyGoodFileCount;
        private OdbcConnection cnDB;

        public AWSLogParser()
        {
            InitializeComponent();
            labelPath.Text = Properties.Settings.Default.LastPath;
            labelStatus.Text = "";
        }

        private void buttonChooseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog chooseDlg = new FolderBrowserDialog();
            if (Properties.Settings.Default.LastPath.Length > 0)
                chooseDlg.SelectedPath = Properties.Settings.Default.LastPath;

            if (chooseDlg.ShowDialog() == DialogResult.OK)
            {
                string path = chooseDlg.SelectedPath;
                Properties.Settings.Default.LastPath = path;
                Properties.Settings.Default.Save();

                labelPath.Text = path;
                Console.WriteLine(path);
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            totalFileCount = 0;
            partialGoodFileCount = 0;
            fullyGoodFileCount = 0;
            labelStatus.Text = "";

            cnDB = new OdbcConnection(connectStr);

            try
            {
                cnDB.Open();
            }
            catch (OdbcException ex)
            {
                labelStatus.Text = "ODBC Connection Open Exception:" + ex.Message + "\n\n" + "StackTrace: \n\n" + ex.StackTrace;
                return;
            }

            string path = Properties.Settings.Default.LastPath;
            if (path.Length > 0)
            {
                ProcessFolder(path);
                Console.WriteLine("DONE");
            }

            cnDB.Close();
        }

        private void buttonConsolidateLogs_Click(object sender, EventArgs e)
        {
			Console.WriteLine("Not implemented yet");
        }

        private void readFile(string file)
        {
            bool gotGoodLine = false;
            try
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    int count = 0;
                    string line;
                    
                    while ((line = sr.ReadLine()) != null)
                    {
                        //Console.WriteLine(line);

                        int state = 0;
                        string field = "";
                        List<string> tokens = new List<string>();
                        for (int i = 0; i < line.Length; i++)
                        {
                            // use state machine to parse as it works a lot easier than regex due to complexity of field structure
                            switch (state)
                            {
                                case 0:
                                    if (line[i] == '[')
                                        state = 1;
                                    else if (line[i] == '\"')
                                        state = 2;
                                    else if (line[i] == ' ')
                                    {
                                        if (field.Length > 0)
                                        {
                                            tokens.Add(field);
                                            field = "";
                                        }
                                    }
                                    else
                                        field += line[i];
                                    break;
                                case 1:
                                    if (line[i] == ']')
                                    {
                                        tokens.Add(field);
                                        field = "";
                                        state = 0;
                                    }
                                    else
                                        field += line[i];
                                    break;
                                case 2:
                                    if (line[i] == '\"' && i < (line.Length - 2) && line[i + 1] == ' ')
                                    {
                                        tokens.Add(field);
                                        field = "";
                                        state = 0;
                                    }
                                    else
                                        field += line[i];
                                    break;
                            }
                        }

                        if (field.Length > 0)
                        {
                            tokens.Add(field);
                            field = "";
                        }

                        int column = 0;
                        DateTime date;
                        const int expectedColumns = 18;

                        string sql = "insert into aws_stats values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
                        OdbcCommand cmd = new OdbcCommand(sql, cnDB);
                        OdbcParameter param;

                        try
                        {

                            //http://docs.aws.amazon.com/AmazonS3/latest/dev/LogFormat.html
                            foreach (string t in tokens)
                            {
                                string token = null;
                                if (!t.Equals("-"))
                                    token = t;
                                string paramKey = "p" + column;
                                switch (column)
                                {
                                    case 0:
                                        if (token != null && token.Length > 255)
                                            throw new Exception("Column too long " + column);

                                        // uuid (bucket owner)
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 1:
                                        if (token != null && token.Length > 255)
                                            throw new Exception("Column too long " + column);
                                        // bucket
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 2:
                                        // time/date
                                        date = parseDate(token);
                                        param = new OdbcParameter(paramKey, OdbcType.DateTime);
                                        param.Value = date;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 3:
                                        if (token != null && token.Length > 255)
                                            throw new Exception("Column too long " + column);
                                        // remote IP address
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 4:
                                        if (token != null && token.Length > 255)
                                            throw new Exception("Column too long " + column);
                                        // uuid of requestor
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 5:
                                        if (token != null && token.Length > 255)
                                            throw new Exception("Column too long " + column);
                                        // request id
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 6:
                                        if (token != null && token.Length > 255)
                                            throw new Exception("Column too long " + column);
                                        // REST type/SOAP operation
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 7:
                                        if (token != null && token.Length > 255)
                                            throw new Exception("Column too long " + column);
                                        // filepath/file requested of URL
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 8:
                                        if (token != null && token.Length > 255)
                                            throw new Exception("Column too long " + column);
                                        // Full GET request
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 9:
                                        // HTTP status/server result code i.e. 200
                                        param = new OdbcParameter(paramKey, OdbcType.Int);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 10:
                                        if (token != null && token.Length > 255)
                                            throw new Exception("Column too long " + column);
                                        // error code, if any
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 11:
                                        // bytes sent
                                        param = new OdbcParameter(paramKey, OdbcType.Int);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 12:
                                        // object size
                                        param = new OdbcParameter(paramKey, OdbcType.Int);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 13:
                                        // total time ms
                                        param = new OdbcParameter(paramKey, OdbcType.Int);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 14:
                                        // turnaround time
                                        param = new OdbcParameter(paramKey, OdbcType.Int);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 15:
                                        if (token != null && token.Length > 1024)
                                            throw new Exception("Column too long " + column);
                                        // referrer
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 16:
                                        if (token != null && token.Length > 1024)
                                            throw new Exception("Column too long " + column);
                                        // user agent
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                    case 17:
                                        if (token != null && token.Length > 255)
                                            throw new Exception("Column too long " + column);
                                        // version id of request
                                        param = new OdbcParameter(paramKey, OdbcType.VarChar);
                                        param.Value = token;
                                        cmd.Parameters.Add(param);
                                        break;
                                }

                                //Console.Write(token);

                                //if (column < (expectedColumns - 1))
                                //    Console.Write(",");

                                column++;
                            }
                            //Console.WriteLine();

                            if (column != expectedColumns)
                                throw new Exception("Expecting " + expectedColumns + " fields and got " + column + " : " + line);

                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                        }
                        catch (OdbcException oex)
                        {
                            Console.WriteLine("ODBC Error " + oex.Message + " File: " + file + " Line: " + line);
                            cmd.Dispose();
                        }

                        if (!gotGoodLine)
                        {
                            gotGoodLine = true;
                        }
                        count++;
                    }
                    Console.WriteLine("Lines in file: " + count);

                    totalFileCount++;

                    if (gotGoodLine)
                        fullyGoodFileCount++;

                    labelStatus.Text = "Total: " + totalFileCount + ", Partial Good: " + partialGoodFileCount + ", Fully Good: " + fullyGoodFileCount;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem with file " + file + ": " + ex.Message);
                if (gotGoodLine)
                    partialGoodFileCount++;
            }
        }

        public static DateTime parseDate(string date)
        {
            //Console.Write("DATE PARTS:");
            char[] delimiters = new char[] { ':', '/', ' ' };
            string[] dp = date.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            /*
            foreach (string part in dp)
            {
                Console.Write(part + ",");
            }
            Console.WriteLine("PARTS DONE");
             */
            const int expectedLen = 7;
            if (dp.Length == expectedLen)
            {
                string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};
                int month = 0;
                for (int i = 1; i <= months.Length; i++)
                {
                    if (months[i - 1].Equals(dp[1]))
                    {
                        month = i;
                        break;
                    }
                }

                if (month == 0)
                {
                    throw new Exception("Unknown month in file " + dp[1]);
                }

                DateTimeKind k = dp[6].Equals("+0000") ? DateTimeKind.Utc : DateTimeKind.Unspecified;

                DateTime dt = new DateTime(Int32.Parse(dp[2]), month, Int32.Parse(dp[0]), 
                    Int32.Parse(dp[3]), Int32.Parse(dp[4]), Int32.Parse(dp[5]), 0, k);
                return dt;
            }
            else
            {
                throw new Exception("Incorrect number of data parts " + dp.Length + " expecting " + expectedLen);
            }
        }

        public void ProcessFolder(string dir)
        {
			string[] files = null;
			try
			{
				files = System.IO.Directory.GetFiles(dir);
			}
			catch (UnauthorizedAccessException e)
			{
				Console.WriteLine(e.Message);
				return;
			}
			catch (System.IO.DirectoryNotFoundException e)
			{
				Console.WriteLine(e.Message);
				return;
			}

			foreach (string file in files)
			{
				try
				{
					readFile(file);

					//System.IO.FileInfo fi = new System.IO.FileInfo(file);
					//Console.WriteLine("{0}: {1}, {2}", fi.Name, fi.Length, fi.CreationTime);
				}
				catch (System.IO.FileNotFoundException e)
				{
					Console.WriteLine(e.Message);
					continue;
				}
			}
			
			string[] subDirs;
			try
			{
				subDirs = System.IO.Directory.GetDirectories(dir);
				foreach (string str in subDirs)
				{
		            if (!System.IO.Directory.Exists(str))
					{
						throw new ArgumentException();
					}

					ProcessFolder(str);
				}
			}
			catch (UnauthorizedAccessException e)
			{
				Console.WriteLine(e.Message);
			}
			catch (System.IO.DirectoryNotFoundException e)
			{
				Console.WriteLine(e.Message);
			}
        }
    }
}