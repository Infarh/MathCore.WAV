using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace MathCore.WAV.Service
{
    /// <summary>Класс вспомогательных методов-расширений</summary>
    internal static class Extensions
    {
        /// <summary>Выполнить действие над разрушимым объектом, получить результат и освободить ресурсы по завершении</summary>
        /// <typeparam name="T">Тип объекта, над которым требуется выполнить операцию извлечения данных</typeparam>
        /// <typeparam name="TValue">Тип ожидаемого результата</typeparam>
        /// <param name="obj">Объект, над которым требуется выполнить действие</param>
        /// <param name="Selector">Метод, осуществляющий извлечение данных из объекта</param>
        /// <returns>Полученный результат</returns>
        public static TValue Using<T, TValue>(this T obj, Func<T, TValue> Selector)
            where T : IDisposable
        {
            using (obj) return Selector(obj);
        }

        /// <summary>Выполнить асинхронную процедуру записи массива байт данных в объект записи двоичных данных</summary>
        /// <param name="Writer">Объект записи двоичных данных</param>
        /// <param name="buffer">Записываемый массив байт данных</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Задача асинхронной записи массива байт данных</returns>
        public static async Task WriteAsync(this BinaryWriter Writer, byte[] buffer, CancellationToken Cancel = default)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            await Writer.BaseStream.WriteAsync(buffer, 0, buffer.Length, Cancel);
        }

        /// <summary>Выполнить асинхронную процедуру записи целочисленного 4-байтового значения в объект записи двоичных данных</summary>
        /// <param name="Writer">Объект записи двоичных данных</param>
        /// <param name="value">Целочисленное 4-байтовое значение для записи</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Задача асинхронной записи целочисленного 4-байтового значения</returns>
        public static async Task WriteAsync(this BinaryWriter Writer, int value, CancellationToken Cancel = default) =>
            await Writer.BaseStream.WriteAsync(
                buffer: new[]
                {
                    (byte)value,
                    (byte)(value >> 8),
                    (byte)(value >> 16),
                    (byte)(value >> 24)
                },
                offset: 0,
                count: 4,
                cancellationToken: Cancel);

        /// <summary>Выполнить асинхронную процедуру записи целочисленного 2-байтового значения в объект записи двоичных данных</summary>
        /// <param name="Writer">Объект записи двоичных данных</param>
        /// <param name="value">Целочисленное 2-байтовое значение для записи</param>
        /// <param name="Cancel">Признак отмены асинхронной операции</param>
        /// <returns>Задача асинхронной записи целочисленного 2-байтового значения</returns>
        public static async Task WriteAsync(this BinaryWriter Writer, short value, CancellationToken Cancel = default) =>
            await Writer.BaseStream.WriteAsync(
                buffer: new[]
                {
                    (byte)value,
                    (byte) ((uint) value >> 8)
                },
                offset: 0,
                count: 2,
                cancellationToken: Cancel);

        /// <summary>Вычислить хеш-сумму значений массива c учётом индекса значений</summary>
        /// <typeparam name="T">Тип объектов массива</typeparam>
        /// <param name="array">Массив, хеш-сумму значений ячеек которого требуется вычислить</param>
        /// <returns>Шех-сумма длины и всех ячеек массива</returns>
        public static int GetItemsHashCode<T>(this T[] array)
        {
            if (array is null) return 0;
            var length = array.Length;
            if (length == 0) return 0;
            var hash = length.GetHashCode();
            for (var i = 0; i < length; i++)
                hash = unchecked((((hash * 397) ^ i) * 397) ^ array[i].GetHashCode());

            return hash;
        }

        public static IEnumerable<T[]> AsBlockEnumerable<T>(this IEnumerable<T>[] Series)
        {
            var series_length = Series.Length;
            IEnumerator<T>[] enumerators = null;
            try
            {
                enumerators = Series.Select(e => e.GetEnumerator()).ToArray();
                while (enumerators.All(e => e.MoveNext()))
                {
                    var values = enumerators.Select(e => e.Current).ToArray();
                    yield return values;
                }
            }
            finally
            {
                if(enumerators != null)
                    for (var i = 0; i < series_length; i++)
                        try { enumerators[i]?.Dispose(); }
                        catch
                        {
                            // ignored
                        }
            }
        }

        public static IEnumerable<T[]> AsBlockEnumerableWithSingleArray<T>(this IEnumerable<T>[] Series)
        {
            var series_length = Series.Length;
            IEnumerator<T>[] enumerators = null;
            try
            {
                enumerators = Series.Select(e => e.GetEnumerator()).ToArray();
                var result = new T[series_length];
                while (enumerators.All(e => e.MoveNext()))
                {
                    for (var i = 0; i < series_length; i++) 
                        result[i] = enumerators[i].Current;
                    yield return result;
                }
            }
            finally
            {
                if (enumerators != null)
                    for (var i = 0; i < series_length; i++)
                        try { enumerators[i]?.Dispose(); }
                        catch
                        {
                            // ignored
                        }
            }
        }
    }
}