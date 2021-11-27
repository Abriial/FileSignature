using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FileSignature
{
    class Worker
    {
        //Количество потоков по умолчанию
        private int threadCount = 1;
        //Текущий блок
        private int currentBlock = 1;
        //Словарь для вывода
        private Dictionary<int, byte[]> dictionary = new Dictionary<int, byte[]>();
        //Список для потоков
        private List<Thread> threads = new List<Thread>();
        //хэшер
        private Hasher256 hasher;
        //Количество блоков
        private int blockCount = 0;
        //Логгер
        Logger logger;

        //Конструктор
        public Worker(int inputThreadCount, Hasher256 inputHasher, Logger inputLogger)
        {
            threadCount = inputThreadCount;
            hasher = inputHasher;
            logger = inputLogger;
        }

        public void Work(object arr)
        {
            //Входной буфер
            InputFileBuffer arrForByte = arr as InputFileBuffer;

            //Пока не превышен размер блока
            while(arrForByte.HasOneBlock())
            {
                try
                {
                    //Обнуляем свойства и переменные
                    blockCount = 0;
                    dictionary.Clear();
                    threads.Clear();

                    for (int i = 0; i < threadCount; i++)
                    {
                        if (arrForByte.HasOneBlock())
                        {
                            //Получаем блок
                            byte[] block = arrForByte.GetBlock();
                            int index = currentBlock;

                            //Запускаем поток с параметром
                            Thread thread = new Thread(() => GetBlockHash(block, index));
                            //Добавляем поток в список
                            threads.Add(thread);
                            //Запускаем поток
                            thread.Start();

                            currentBlock++;
                            blockCount++;
                        }
                    }

                    foreach (Thread th in threads)
                    {
                        th.Join();
                    }

                    int id = 0;
                    foreach (byte[] block in GetBlockOrder())
                    {
                        Output(currentBlock - blockCount + id, block);
                        id++;
                    }
                }
                catch (Exception ex) 
                {
                    logger.WriteToLog(ex.Message + "\n" + ex.StackTrace, "Block: " + currentBlock);
                    Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                }
            }

            if (arrForByte.NotFinished() > 0)
            {
                Output(currentBlock, hasher.ComputeHash(arrForByte.GetLastBlock()));
            }
        }

        //Получение хэша
        public void GetBlockHash(object block, object index)
        {
            try
            {
                lock (dictionary)
                {
                    dictionary.Add((int)index, hasher.ComputeHash((byte[])block));
                    Monitor.Pulse(dictionary);
                }
            }
            catch (Exception ex)
            {
                logger.WriteToLog(ex.Message + "\n" + ex.StackTrace, "Block: " + currentBlock);
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }
        }

        //Получаем порядок блоков
        private List<byte[]> GetBlockOrder()
        {
            List<byte[]> block = new List<byte[]>();

            try
            {
                for (int i = 0; i < blockCount; i++)
                {
                    block.Add(dictionary[currentBlock - blockCount + i]);
                }
            }
            catch (Exception ex)
            {
                logger.WriteToLog(ex.Message + "\n" + ex.StackTrace, "Block: " + currentBlock);
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
            }

            return block;
        }

        private void Output(int blockNum, byte[] hash)
        {
            Console.Write(blockNum + " ");

            for (int i = 0; i < hash.Length; i++)
            {
                Console.Write(String.Format("{0:X2}", hash[i]));
                if ((i % 4) == 3) Console.Write(" ");
            }
            Console.WriteLine();
        }

    }
}
