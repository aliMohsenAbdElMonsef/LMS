using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using Microsoft.EntityFrameworkCore.Metadata;

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

       
        public virtual async Task<ServiceResponseDTO<TReadDTO>> CreateAsync(TCreateDTO dto)
        {
            try
            {
                TEntity entity = MapToEntity(dto);
                await GetRepo().CreateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                var ReadEntity = MapToReadDTO(entity);
                ServiceResponseDTO<TReadDTO> response = new ServiceResponseDTO<TReadDTO>
                {
                    Data = ReadEntity,
                    Success = true,
                    Message = "Entity created successfully."
                };
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating entity: {ex.Message}", ex);
            }
        }

        public virtual async Task<ServiceResponseDTO<TReadDTO>> UpdateAsync(TUpdateDTO dto)
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
                var ReadEntity = MapToReadDTO(updatedEntity);
                ServiceResponseDTO<TReadDTO> response = new ServiceResponseDTO<TReadDTO>
                {
                    Data = ReadEntity,
                    Success = true,
                    Message = "Entity updated successfully."
                };

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating entity: {ex.Message}", ex);
            }
        }

        public virtual async Task<ServiceResponseDTO<TReadDTO>> DeleteAsync(string id)
        {
            try
            {
                var entity = await GetRepo().FindByIdAsync(id);
                if (entity == null)
                {
                    ServiceResponseDTO<TReadDTO> Errorresponse = new ServiceResponseDTO<TReadDTO>
                    {
                        Success = false,
                        Message = "Entity Not Found."
                    };
                    return Errorresponse;
                }

                await GetRepo().DeleteWithIDAsync(id);
                await _unitOfWork.SaveChangesAsync();

                ServiceResponseDTO<TReadDTO> response = new ServiceResponseDTO<TReadDTO>
                {
                    Success = true,
                    Message = "Entity Deleted Succesfully."
                };
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting entity with id '{id}': {ex.Message}", ex);
            }
        }

        public virtual async Task<ServiceResponseDTO<IEnumerable<TReadDTO>>> GetAllAsync()
        {
            try
            {
                var entities = await GetRepo().GetAllAsync();

                ServiceResponseDTO<IEnumerable<TReadDTO>> response = new ServiceResponseDTO<IEnumerable<TReadDTO>>
                {
                    Data = entities.Select(e => MapToReadDTO(e)).ToList(),
                    Success = true,
                    Message = "Entities retrieved successfully."
                };

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving entities: {ex.Message}", ex);
            }
        }

        public virtual async Task<ServiceResponseDTO<TReadDTO>> GetByIdAsync(string id)
        {
            try
            {
                var entity = await GetRepo().FindByIdAsync(id);
                if (entity == null)
                {
                    ServiceResponseDTO<TReadDTO> Errorresponse = new ServiceResponseDTO<TReadDTO>
                    {
                        Success = false,
                        Message = "Entity Not Found."
                    };
                    return Errorresponse;
                }

                var ReadEntity = MapToReadDTO(entity);
                ServiceResponseDTO<TReadDTO> response = new ServiceResponseDTO<TReadDTO>
                {
                    Data = ReadEntity,
                    Success = true,
                    Message = "Entity retrieved successfully."
                };
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving entity with id '{id}': {ex.Message}", ex);
            }
        }


    }
}
