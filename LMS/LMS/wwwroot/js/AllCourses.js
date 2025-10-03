document.addEventListener('DOMContentLoaded', function() {
    const courseSearch = document.getElementById('courseSearch');
    const clearSearch = document.getElementById('clearSearch');
    const clearSearchBtn = document.getElementById('clearSearchBtn');
    const coursesGrid = document.getElementById('coursesGrid');
    const noResults = document.getElementById('noResults');
    const searchResultsCount = document.getElementById('searchResultsCount');
    const courseItems = document.querySelectorAll('.course-item');

    if (!courseSearch || !coursesGrid) {
        console.error('Required elements not found');
        return;
    }

    const totalCourses = document.getElementById('totalCourses');
    const freeCourses = document.getElementById('freeCourses');
    const beginnerCourses = document.getElementById('beginnerCourses');

    const originalCourseCount = @Model.Count();
    const originalFreeCount = @Model.Count(c => c.IsFree);
    const originalBeginnerCount = @Model.Count(c => c.Level == LMS.Models.DataModels.Levels.Beginner);

    function performSearch() {
        const searchTerm = courseSearch.value.toLowerCase().trim();
        let visibleCount = 0;
        let freeCount = 0;
        let beginnerCount = 0;

        if (searchTerm === '') {
            courseItems.forEach(item => {
                item.style.display = 'block';
                visibleCount++;
                const courseData = getCourseDataFromItem(item);
                if (courseData.isFree) freeCount++;
                if (courseData.isBeginner) beginnerCount++;
            });

            coursesGrid.style.display = 'flex';
            noResults.style.display = 'none';
            
            if (clearSearch) {
                clearSearch.classList.add('d-none');
            }
            
            searchResultsCount.textContent = '';

            updateStatistics(originalCourseCount, originalFreeCount, originalBeginnerCount);
        } else {
            courseItems.forEach(item => {
                const courseName = item.getAttribute('data-name');
                const courseDescription = item.getAttribute('data-description');

                const matchesName = courseName.includes(searchTerm);
                const matchesDescription = courseDescription && courseDescription.includes(searchTerm);

                if (matchesName || matchesDescription) {
                    item.style.display = 'block';
                    visibleCount++;

                    highlightText(item, searchTerm);

                    const courseData = getCourseDataFromItem(item);
                    if (courseData.isFree) freeCount++;
                    if (courseData.isBeginner) beginnerCount++;
                } else {
                    item.style.display = 'none';
                }
            });

            if (visibleCount > 0) {
                coursesGrid.style.display = 'flex';
                noResults.style.display = 'none';
                searchResultsCount.textContent = `Found ${visibleCount} course${visibleCount !== 1 ? 's' : ''} matching "${searchTerm}"`;
                updateStatistics(visibleCount, freeCount, beginnerCount);
            } else {
                coursesGrid.style.display = 'none';
                noResults.style.display = 'block';
                searchResultsCount.textContent = `No courses found matching "${searchTerm}"`;
                updateStatistics(0, 0, 0);
            }

            if (clearSearch) {
                clearSearch.classList.remove('d-none');
            }
        }
    }

    function highlightText(courseItem, searchTerm) {
        const nameElement = courseItem.querySelector('.course-name');
        const descriptionElement = courseItem.querySelector('.course-description');

        nameElement.innerHTML = nameElement.textContent;
        if (descriptionElement) {
            descriptionElement.innerHTML = descriptionElement.textContent;
        }

        if (nameElement.textContent.toLowerCase().includes(searchTerm)) {
            const regex = new RegExp(`(${searchTerm})`, 'gi');
            nameElement.innerHTML = nameElement.textContent.replace(regex, '<span class="highlight">$1</span>');
        }

        if (descriptionElement && descriptionElement.textContent.toLowerCase().includes(searchTerm)) {
            const regex = new RegExp(`(${searchTerm})`, 'gi');
            descriptionElement.innerHTML = descriptionElement.textContent.replace(regex, '<span class="highlight">$1</span>');
        }
    }

    function getCourseDataFromItem(courseItem) {
        const isFree = courseItem.querySelector('.course-free') !== null;
        const level = courseItem.querySelector('.course-level').textContent;

        return {
            isFree: isFree,
            isBeginner: level === 'Beginner'
        };
    }

    function updateStatistics(total, free, beginner) {
        if (totalCourses) totalCourses.textContent = total;
        if (freeCourses) freeCourses.textContent = free;
        if (beginnerCourses) beginnerCourses.textContent = beginner;
    }

    function clearSearchHandler() {
        courseSearch.value = '';
        performSearch();
        courseSearch.focus();
    }

    courseSearch.addEventListener('input', function() {
        if (clearSearch) {
            clearSearch.classList.toggle('d-none', this.value === '');
        }
        performSearch();
    });

    courseSearch.addEventListener('keyup', function(e) {
        if (e.key === 'Escape') {
            clearSearchHandler();
        }
    });

    if (clearSearch) {
        clearSearch.addEventListener('click', clearSearchHandler);
    }
    
    if (clearSearchBtn) {
        clearSearchBtn.addEventListener('click', clearSearchHandler);
    }

    const courseCards = document.querySelectorAll('.course-card');
    courseCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-5px)';
        });
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });

    performSearch();
});