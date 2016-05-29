﻿using System;
using System.Reflection;

namespace VroomJs
{
    public partial class JsContext : IDisposable
    {
        private bool _disposed;
        private readonly int _id;
        private readonly JsEngine _engine;
        private readonly JsContextSafeHandle _context;
        private readonly Action<int> _notifyDispose;
        readonly IKeepAliveStore _keepalives;
        readonly JsConvert _convert;

        internal JsContext(int id, JsEngine engine, JsEngineSafeHandle engineHandle, Action<int> notifyDispose)
        {
            _id = id;
            _engine = engine;

            _keepalives = new KeepAliveDictionaryStore();
            _context = new JsContextSafeHandle(engineHandle, id);
            _convert = new JsConvert(this);

            _notifyDispose = notifyDispose;
        }

        internal JsValue KeepAliveValueOf(int slot)
        {
            throw new NotImplementedException();
        }

        internal JsValue KeepAliveInvoke(int slot, JsValue args)
        {
            throw new NotImplementedException();
        }

        internal JsValue KeepAliveSetPropertyValue(int slot, string name, JsValue value)
        {
            throw new NotImplementedException();
        }

        internal JsValue KeepAliveGetPropertyValue(int slot, string name)
        {
            throw new NotImplementedException();
        }

        internal JsValue KeepAliveDeleteProperty(int slot, string name)
        {
            throw new NotImplementedException();
        }

        internal JsValue KeepAliveEnumerateProperties(int slot)
        {
            throw new NotImplementedException();
        }

        public object GetVariable(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue v = Native.jscontext_get_variable(_context, name);
            object res = _convert.FromJsValue(v);

            Native.jsvalue_dispose(v);

            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }

        public void SetVariable(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue a = _convert.ToJsValue(value);
            JsValue b = Native.jscontext_set_variable(_context, name, a);

            Native.jsvalue_dispose(a);
            Native.jsvalue_dispose(b);
            // TODO: Check the result of the operation for errors.
        }

        public object Execute(string code, string name = null, TimeSpan? executionTimeout = null)
        {
            if (code == null)
                throw new ArgumentNullException("code");

            CheckDisposed();
            
            var v = Native.jscontext_execute(_context, code, name ?? "<Unnamed Script>");
            var result = _convert.FromJsValue(v);
            Native.jsvalue_dispose(v);

            return result;
        }

        #region Keep-alive management and callbacks.

        internal int KeepAliveAdd(object obj)
        {
            return _keepalives.Add(obj);
        }

        internal object KeepAliveGet(int slot)
        {
            return _keepalives.Get(slot);
        }

        internal void KeepAliveRemove(int slot)
        {
            _keepalives.Remove(slot);
        }

        #endregion

        #region IDisposable implementation

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(JsEngine));
        }

        ~JsContext()
        {
            if (!_disposed)
                Dispose(false);
        }

        public void Dispose()
        {
            CheckDisposed();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            CheckDisposed();
            
            _disposed = true;

            if (disposing)
            {
                _notifyDispose(_id);
                _context.Dispose();
            }
        }

        #endregion
    }
}