using System;
using System.Collections.Generic;
using System.Text;

namespace Yago.DataAcsess.Abstract
{
    public interface IGenericDal<T> where T : class
    {
        void Insert(T entity);
        void Delete(T entity);
        void Update(T entity);
        List<T> GetAll();
        T GetById(int id);
    }
}
