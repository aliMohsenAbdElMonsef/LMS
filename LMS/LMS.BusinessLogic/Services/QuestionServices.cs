using AutoMapper;
using LMS.BusinessLogic.Contracts;
using LMS.BusinessLogic.Contracts.Services;
using LMS.BusinessLogic.DTOs.Question;
using LMS.BusinessLogic.DTOs.Responses;
using LMS.DataAccess.Contracts;
using Domain.Entities.MainEntities;

namespace LMS.BusinessLogic.Services
{
    internal class QuestionServices : IQuestionServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuestionServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ServiceResponseDTO<ReadQuestionDTO>> CreateAsync(CreateQuestionDTO dto)
        {
            var question = _mapper.Map<Question>(dto);
            await _unitOfWork.Questions.CreateAsync(question);
            await _unitOfWork.SaveChangesAsync();
            
            return new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = true,
                Data = _mapper.Map<ReadQuestionDTO>(question)
            };
        }

        public async Task<ServiceResponseDTO<ReadQuestionDTO>> DeleteAsync(string id)
        {
            var question = await _unitOfWork.Questions.FindByIdAsync(id);
            if (question == null)
            {
                return new ServiceResponseDTO<ReadQuestionDTO>
                {
                    Success = false,
                    Message = "Entity Not Found."
                };
            }

            await _unitOfWork.Questions.DeleteByEntityAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = true,
                Message = "Entity Deleted Succesfully.",
                Data = _mapper.Map<ReadQuestionDTO>(question)
            };
        }

        public async Task<ServiceResponseDTO<IEnumerable<ReadQuestionDTO>>> GetAllAsync()
        {
            var questions = await _unitOfWork.Questions.GetAllAsync();
            return new ServiceResponseDTO<IEnumerable<ReadQuestionDTO>>
            {
                Success = true,
                Data = _mapper.Map<IEnumerable<ReadQuestionDTO>>(questions)
            };
        }

        public async Task<ServiceResponseDTO<ReadQuestionDTO>> GetByIdAsync(string id)
        {
            var question = await _unitOfWork.Questions.FindByIdAsync(id);
            if (question == null)
            {
                return new ServiceResponseDTO<ReadQuestionDTO>
                {
                    Success = false,
                    Message = "Entity Not Found."
                };
            }

            return new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = true,
                Data = _mapper.Map<ReadQuestionDTO>(question)
            };
        }

        public async Task<ServiceResponseDTO<ReadQuestionDTO>> UpdateAsync(UpdateQuestionDTO dto)
        {
            var question = await _unitOfWork.Questions.FindByIdAsync(dto.Id);
            if (question == null)
            {
                return new ServiceResponseDTO<ReadQuestionDTO>
                {
                    Success = false,
                    Message = "Entity Not Found."
                };
            }

            _mapper.Map(dto, question);
            await _unitOfWork.Questions.UpdateAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return new ServiceResponseDTO<ReadQuestionDTO>
            {
                Success = true,
                Data = _mapper.Map<ReadQuestionDTO>(question)
            };
        }
    }
}
