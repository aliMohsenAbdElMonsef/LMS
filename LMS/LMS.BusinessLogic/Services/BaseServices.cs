using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LMS.BusinessLogic.Contracts;
using LMS.DataAcess.Contracts;
namespace LMS.BusinessLogic.Services
{
    internal abstract class BaseServices<TEntity,TReadDTO, TCreateDTO, TUpdateDTO> : IBaseService<TReadDTO, TCreateDTO, TUpdateDTO> where TReadDTO : class where TCreateDTO : class where TUpdateDTO : class where TEntity : class
    {
        protected readonly IUnitOfWork _unitOfWork;

        public BaseServices(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
        protected abstract TEntity MapToEntity(TCreateDTO dto); 
        protected abstract TEntity UpdateToEntity(TUpdateDTO dto, TEntity existingEntity); 
        protected abstract TReadDTO MapToReadDTO(TEntity entity); 
        protected abstract IBaseRepository<TEntity,string> GetRepo();
        protected abstract string GetIdFromUpdateDTO(TUpdateDTO dto); 

        public TReadDTO Create(TCreateDTO dto)
        {
            TEntity entity = MapToEntity(dto);

            GetRepo().Create(entity);

            _unitOfWork.SaveChanges();

            return MapToReadDTO(entity);
        }

        public void Delete(string id)
        {
            try
            {
                GetRepo().Delete(id);

                _unitOfWork.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting entity with id '{id}': {ex.Message}");
            }
        }

        public IEnumerable<TReadDTO> GetAll()
        {
            var entities = GetRepo().GetAll();
            return entities.Select(e => MapToReadDTO(e)).ToList();
        }

        public TReadDTO GetById(string id)
        {
            var entity = GetRepo().FindByID(id);
            if (entity == null)
            {
                throw new Exception($"Entity with id '{id}' not found.");
            }
            return MapToReadDTO(entity);
        }

        public TReadDTO Update(TUpdateDTO dto)
        {
            var existingEntity = GetRepo().FindByID(GetIdFromUpdateDTO(dto));
            
            if (existingEntity == null)
            {
                throw new Exception($"Entity with id '{GetIdFromUpdateDTO(dto)}' not found.");
            }

            var updatedEntity = UpdateToEntity(dto, existingEntity);

            GetRepo().Update(updatedEntity);

            _unitOfWork.SaveChanges();

            return MapToReadDTO(updatedEntity);
        }
    }
}
