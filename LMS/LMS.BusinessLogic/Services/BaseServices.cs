using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.BusinessLogic.Contracts;
using LMS.DataAcess.Contracts;

namespace LMS.BusinessLogic.Services
{
    internal abstract class BaseServices<TEntity, TReadDTO, TCreateDTO, TUpdateDTO> : IBaseService<TReadDTO, TCreateDTO, TUpdateDTO>
        where TReadDTO : class
        where TCreateDTO : class
        where TUpdateDTO : class
        where TEntity : class
    {
        protected readonly IUnitOfWork _unitOfWork;

        public BaseServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        protected abstract TEntity MapToEntity(TCreateDTO dto);

        protected abstract TEntity UpdateToEntity(TUpdateDTO dto, TEntity existingEntity);
        protected abstract TReadDTO MapToReadDTO(TEntity entity);

        protected abstract IBaseRepository<TEntity, string> GetRepo();

        protected abstract string GetIdFromUpdateDTO(TUpdateDTO dto);

       
        public virtual async Task<TReadDTO> CreateAsync(TCreateDTO dto)
        {
            try
            {
                TEntity entity = MapToEntity(dto);
                await GetRepo().CreateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                return MapToReadDTO(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating entity: {ex.Message}", ex);
            }
        }

        public virtual async Task<TReadDTO> UpdateAsync(TUpdateDTO dto)
        {
            try
            {
                var id = GetIdFromUpdateDTO(dto);

                var existingEntity = await GetRepo().FindByIdAsync(id);
                if (existingEntity == null)
                    throw new Exception($"Entity with id '{id}' not found.");

                var updatedEntity = UpdateToEntity(dto, existingEntity);
                await GetRepo().UpdateAsync(updatedEntity);
                await _unitOfWork.SaveChangesAsync();

                return MapToReadDTO(updatedEntity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating entity: {ex.Message}", ex);
            }
        }

        public virtual async Task DeleteAsync(string id)
        {
            try
            {
                var entity = await GetRepo().FindByIdAsync(id);
                if (entity == null)
                    throw new Exception($"Entity with id '{id}' not found.");

                await GetRepo().DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting entity with id '{id}': {ex.Message}", ex);
            }
        }

        public virtual async Task<IEnumerable<TReadDTO>> GetAllAsync()
        {
            try
            {
                var entities = await GetRepo().GetAllAsync();
                return entities.Select(e => MapToReadDTO(e)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving entities: {ex.Message}", ex);
            }
        }

        public virtual async Task<TReadDTO> GetByIdAsync(string id)
        {
            try
            {
                var entity = await GetRepo().FindByIdAsync(id);
                if (entity == null)
                    throw new Exception($"Entity with id '{id}' not found.");

                return MapToReadDTO(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving entity with id '{id}': {ex.Message}", ex);
            }
        }
    }
}
