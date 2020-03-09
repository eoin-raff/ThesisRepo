using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadedDataRequester : MonoBehaviour
{
    static ThreadedDataRequester Instance;
    Queue<ThreadInfo> DataQueue = new Queue<ThreadInfo>();

    private void Awake()
    {
        Instance = FindObjectOfType<ThreadedDataRequester>();
    }

    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        ThreadStart threadStart = delegate
        {
            Instance.DataThread(generateData, callback);
        };
        new Thread(threadStart).Start();
    }

    void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();
        lock (DataQueue)
        {
            DataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }


    private void Update()
    {
        if (DataQueue.Count > 0)
        {
            for (int i = 0; i < DataQueue.Count; i++)
            {
                ThreadInfo threadInfo = DataQueue.Dequeue();
                threadInfo.Callback(threadInfo.parameter);
            }
        }

    }

    struct ThreadInfo
    {
        public readonly Action<object> Callback;
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            Callback = callback;
            this.parameter = parameter;
        }
    }
}
