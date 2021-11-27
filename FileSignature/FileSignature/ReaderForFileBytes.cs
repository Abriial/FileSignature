using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileSignature
{
    class ReaderForFileBytes
    {
        //Буфер загрузки из файла
        private int fileBlockSize;
        //Поток для файла
        private FileStream stream;
        //Буффера для
        private byte[] buf;

        public ReaderForFileBytes(FileStream inputStream, int inputBlockSize)
        {
            fileBlockSize = inputBlockSize * 32; //32 - во сколько раз буфер из файла должен быть больше блока для чтения хэша
            stream = inputStream;
            buf = new byte[fileBlockSize];
        }

        //Задаём блок из файла
        public byte[] SetBlock()
        {
            int readBytes;
            if (stream.Position != stream.Length)
            {
                readBytes = stream.Read(buf, 0, buf.Length);
                if (readBytes < fileBlockSize)
                {
                    byte[] bufFromFile = new byte[readBytes];
                    Array.Copy(buf, bufFromFile, readBytes);
                    return bufFromFile;
                }

                return buf;
            }

            return new byte[0];
        }

        //Задаём байт
        public byte SetByte()
        {
            return (byte)stream.ReadByte();
        }

        //Проверка имеется ли следующий блок
        public bool HasBlock()
        {
            return stream.Position != stream.Length;
        }

        //Возвращаем размер блока
        public int GetBlockSize()
        {
            return fileBlockSize;
        }
    }
}
