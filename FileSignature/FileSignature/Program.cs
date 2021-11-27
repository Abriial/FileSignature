using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileSignature
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Запись ошибок в лог
            Logger logger = new Logger();

            try
            {
                //Проверяем корректность указанных параметров
                string fileName = ""; //Подразумевается полный путь к файлу
                int blockSize = 0; //Подразумевается в байтах
                
                Console.WriteLine("Reading parameters");
                for (int i = 0; i < args.Length; i++)
                {
                    //Если найден флаг имени файла
                    if (args[i] == "-f")
                    {
                        //Если имя файла не пустое
                        if (args[i + 1] != "" && args[i + 1] != null && args[i + 1] != "-b")
                        {
                            fileName = args[i + 1];
                        }
                        else
                        {
                            logger.WriteToLog("Incorrect filename or filepath", "command parameters");
                            throw new Exception("Incorrect filename or filepath");                          
                        }
                    }

                    //Если найден флаг размера блоков
                    if (args[i] == "-b")
                    {
                        if (args[i + 1] != "-f")
                        {
                            try
                            {
                                blockSize = Convert.ToInt32(args[i + 1]);
                            }
                            catch (Exception ex)
                            {
                                logger.WriteToLog("Incorrect block size", "command parameters");
                                throw new Exception("Incorrect block size");
                            }
                        }
                    }
                }

                //Если все данные введены правильно, то выполняем основной цикл
                if (fileName != "" && blockSize != 0)
                {
                    Console.WriteLine("Input data is correct");
                    //Два буфера для загрузки файла
                    InputFileBuffer inputBuf_1 = new InputFileBuffer(blockSize);
                    InputFileBuffer inputBuf_2 = new InputFileBuffer(blockSize);

                    //Текущий буфер, который будет в работе
                    InputFileBuffer workBuffer = inputBuf_1;
                    //Флаг для отслеживания первого буфера
                    bool flagFirst = true;

                    //Создаём буфер для 
                    Worker worker = new Worker(Environment.ProcessorCount, new Hasher256(), logger);
                    Thread bufThread = new Thread(worker.Work);

                    using (FileStream inputFile = File.OpenRead(fileName))
                    {
                        ReaderForFileBytes reader = new ReaderForFileBytes(inputFile, blockSize);

                        while(reader.HasBlock())
                        {
                            if (workBuffer.HasSpaceForAddBlock(reader.GetBlockSize()))
                            {
                                workBuffer.SetBlock(reader.SetBlock());
                            }
                            else if (workBuffer.AllSize() != 0)
                            {
                                workBuffer.SetBlock(reader.SetBlock());
                            }
                            else
                            {
                                if (bufThread.IsAlive) bufThread.Join();

                                bufThread = new Thread(worker.Work);
                                bufThread.Start(workBuffer);

                                workBuffer = inputBuf_2;
                                if (!flagFirst) workBuffer = inputBuf_1;

                                flagFirst = !flagFirst;

                                workBuffer.Clear();
                            }                        
                        }

                        if (bufThread.IsAlive) bufThread.Join();
                        worker.Work(workBuffer);
                    }
                }
                else
                {
                    if (fileName == "")
                    {
                        logger.WriteToLog("Incorrect filename or filepath", "command parameters");
                        throw new Exception("Incorrect filename or filepath");
                    }
                    if (blockSize == 0)
                    {
                        logger.WriteToLog("Incorrect block size", "command parameters");
                        throw new Exception("Incorrect block size");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WriteToLog(ex.Message + "\n" + ex.StackTrace, ex.StackTrace.Split('\r').First().Trim());
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
