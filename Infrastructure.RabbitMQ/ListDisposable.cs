using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.RabbitMQ
{
    public class ListOfDisposables: IDisposable
    {
        private readonly IDisposable[] _disposables;

        public ListOfDisposables(IDisposable[] disposables)
        {
            _disposables = disposables;
        }

        public void Dispose()
        {
            if (_disposables!=null)
            {
                foreach (var disposable in _disposables)
                {
                    disposable?.Dispose();
                }
            }
        }
    }
}
