using Reactive.Bindings.Disposables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LuaMacrosMapper.Models
{
    public class NotificationObject : INotifyPropertyChanged, IDisposable
    {
        private bool disposedValue;
        public event PropertyChangedEventHandler? PropertyChanged;
        protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

        protected virtual void RaisePropertyChanged([CallerMemberName] string? member = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(member));
        }
        protected virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyName)
        {
            var expression = propertyName.Body as MemberExpression;
            if (expression == null)
                throw new ArgumentException();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(expression.Member.Name));
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)
                    Disposable.Dispose();
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、ファイナライザーをオーバーライドします
                // TODO: 大きなフィールドを null に設定します
                disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~NotificationObject()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
