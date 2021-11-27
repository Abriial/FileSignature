using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FileSignature
{
    class InputFileBuffer
    {
        //Размер буфера для чтения файла
        private int bufSize;
        //Размер блока
        private int sizeOneBlock;
        //Массив для хранения блоков
        private ArrayList arr = new ArrayList();
        //Счётчик текущей позиции в массиве
        private int currentPosition = 0;

        //Конструктор InputFileBuffer
        public InputFileBuffer(int inputBlockSize)
        {
            //Присваиваем размер одного блока
            sizeOneBlock = inputBlockSize;
            //Считаем размер буфера исходя из размера заданного блока
            bufSize = inputBlockSize * 8192; //8192 - во сколько раз буфер больше одного блока
            //Задаём количество элементов 
            bufSize = (bufSize / inputBlockSize + 1) * inputBlockSize;
            arr.Capacity = bufSize;
        }

        //Добавление блока
        public void SetBlock(byte[] inputBlock)
        {
            foreach (byte b in inputBlock)
            {
                arr.Add(b);
            }
        }

        //Добавление байта
        public void SetByte(byte b)
        {
            arr.Add(b);
        }

        public int AllSize()
        {
            return (sizeOneBlock - arr.Count % sizeOneBlock) % sizeOneBlock;
        }

        //Проверка превышения размера одного блока
        public bool HasOneBlock()
        {
            return arr.Count - currentPosition >= sizeOneBlock;
        }

        //Получаем блок
        public byte[] GetBlock()
        {
            int position = currentPosition;
            currentPosition += sizeOneBlock;
            return (byte[])arr.GetRange(position, sizeOneBlock).ToArray(typeof(byte));
        }

        //Проверка места для нового блока
        public bool HasSpaceForAddBlock(int size)
        {
            return arr.Count + size <= bufSize;
        }

        //Получаем последний блок
        public byte[] GetLastBlock()
        {
            return (byte[])arr.GetRange(currentPosition, arr.Count - currentPosition).ToArray(typeof(byte));
        }

        //Необработанные байты
        public int NotFinished()
        {
            return arr.Count - currentPosition;
        }

        //Очистка буфера
        public void Clear()
        {
            arr.Clear();
            currentPosition = 0;
        }
    }
}
