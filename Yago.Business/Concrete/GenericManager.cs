using System;
using System.Collections.Generic;
using System.Text;
using Yago.Business.Abstract;
using Yago.DataAcsess.Abstract;

namespace Yago.Business.Concrete
{
    public class GenericManager<T> : IGenericService<T> where T : class
    {
        private readonly IGenericDal<T> _genericRepository;

        public GenericManager(IGenericDal<T> genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public void TInsert(T t)
        {
            _genericRepository.Insert(t);
        }

        public void TUpdate(T t)
        {
            _genericRepository.Update(t);
        }

        public void TDelete(T t)
        {
            _genericRepository.Delete(t);
        }

        public List<T> TGetList()
        {
            return _genericRepository.GetAll();
        }

        public T TGetByID(int id)
        {
            return _genericRepository.GetById(id);
        }
    }
}
