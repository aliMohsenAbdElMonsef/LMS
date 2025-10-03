document.addEventListener('DOMContentLoaded', function () {
    // Skills functionality
    const skillSearch = document.getElementById('skillSearch');
    const searchResults = document.getElementById('searchResults');
    const selectedSkillsList = document.getElementById('selectedSkillsList');
    const selectedSkillIds = document.getElementById('selectedSkillIds');
    const allSkills = @Html.Raw(JsonSerializer.Serialize(Model.AllSkills.Select(s => new { s.Id, s.Name })));

    let selectedSkills = [];

    const prerequisiteSearch = document.getElementById('prerequisiteSearch');
    const prerequisiteSearchResults = document.getElementById('prerequisiteSearchResults');
    const selectedPrerequisitesList = document.getElementById('selectedPrerequisitesList');
    const selectedPrerequisiteIds = document.getElementById('selectedPrerequisiteIds');
    const allCourses = @Html.Raw(JsonSerializer.Serialize(Model.AllCourses.Select(c => new { c.Id, c.Name, c.CourseCode, c.Description })));

    let selectedPrerequisites = [];

    function initializeSelectedSkills() {
        const preSelectedOptions = selectedSkillIds.selectedOptions;
        for (let option of preSelectedOptions) {
            const skillId = parseInt(option.value);
            const skillName = option.text;
            if (!selectedSkills.some(skill => skill.id === skillId)) {
                selectedSkills.push({ id: skillId, name: skillName });
            }
        }
        updateSelectedSkillsDisplay();
    }

    // Skills search
    skillSearch.addEventListener('input', function () {
        const searchTerm = this.value.toLowerCase().trim();

        if (searchTerm.length === 0) {
            searchResults.style.display = 'none';
            return;
        }

        const filteredSkills = allSkills.filter(skill =>
            skill.name.toLowerCase().includes(searchTerm) &&
            !selectedSkills.some(selected => selected.id === skill.id)
        );

        displaySearchResults(filteredSkills, searchResults, 'skill');
    });

    // Prerequisites search
    prerequisiteSearch.addEventListener('input', function () {
        const searchTerm = this.value.toLowerCase().trim();

        if (searchTerm.length === 0) {
            prerequisiteSearchResults.style.display = 'none';
            return;
        }

        const filteredCourses = allCourses.filter(course =>
            (course.name.toLowerCase().includes(searchTerm) ||
                (course.courseCode && course.courseCode.toLowerCase().includes(searchTerm)) ||
                (course.description && course.description.toLowerCase().includes(searchTerm))) &&
            !selectedPrerequisites.some(selected => selected.id === course.id)
        );

        displaySearchResults(filteredCourses, prerequisiteSearchResults, 'prerequisite');
    });

    function displaySearchResults(items, container, type) {
        if (items.length === 0) {
            container.innerHTML = `<div class="search-result-item text-muted">No ${type}s found</div>`;
            container.style.display = 'block';
            return;
        }

        container.innerHTML = items.map(item => `
                    <div class="search-result-item" data-${type}-id="${item.id}" data-${type}-name="${item.name}">
                        <i class="fas fa-plus me-2 text-success"></i>
                        <div>
                            <div class="fw-bold">${item.name}</div>
                            ${item.courseCode ? `<small class="text-muted">${item.courseCode}</small>` : ''}
                            ${item.description ? `<small class="d-block text-muted">${item.description.substring(0, 50)}...</small>` : ''}
                        </div>
                    </div>
                `).join('');

        container.style.display = 'block';
    }

    // Handle skill selection
    searchResults.addEventListener('click', function (e) {
        const resultItem = e.target.closest('.search-result-item');
        if (resultItem) {
            const skillId = parseInt(resultItem.getAttribute('data-skill-id'));
            const skillName = resultItem.getAttribute('data-skill-name');

            addSelectedSkill(skillId, skillName);
            skillSearch.value = '';
            searchResults.style.display = 'none';
        }
    });

    // Handle prerequisite selection
    prerequisiteSearchResults.addEventListener('click', function (e) {
        const resultItem = e.target.closest('.search-result-item');
        if (resultItem) {
            const courseId = parseInt(resultItem.getAttribute('data-prerequisite-id'));
            const courseName = resultItem.getAttribute('data-prerequisite-name');

            addSelectedPrerequisite(courseId, courseName);
            prerequisiteSearch.value = '';
            prerequisiteSearchResults.style.display = 'none';
        }
    });

    function addSelectedSkill(skillId, skillName) {
        if (selectedSkills.some(skill => skill.id === skillId)) {
            return;
        }

        selectedSkills.push({ id: skillId, name: skillName });
        updateSelectedSkillsDisplay();
        updateHiddenSkillsField();
    }

    function addSelectedPrerequisite(courseId, courseName) {
        if (selectedPrerequisites.some(course => course.id === courseId)) {
            return;
        }

        selectedPrerequisites.push({ id: courseId, name: courseName });
        updateSelectedPrerequisitesDisplay();
        updateHiddenPrerequisitesField();
    }

    function removeSelectedSkill(skillId) {
        selectedSkills = selectedSkills.filter(skill => skill.id !== skillId);
        updateSelectedSkillsDisplay();
        updateHiddenSkillsField();
    }

    function removeSelectedPrerequisite(courseId) {
        selectedPrerequisites = selectedPrerequisites.filter(course => course.id !== courseId);
        updateSelectedPrerequisitesDisplay();
        updateHiddenPrerequisitesField();
    }

    function updateSelectedSkillsDisplay() {
        if (selectedSkills.length === 0) {
            selectedSkillsList.innerHTML = '<div class="text-muted">No skills selected yet</div>';
            return;
        }

        selectedSkillsList.innerHTML = selectedSkills.map(skill => `
                    <div class="skill-tag">
                        ${skill.name}
                        <button type="button" class="remove-btn" onclick="removeSelectedSkill(${skill.id})">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                `).join('');
    }

    function updateSelectedPrerequisitesDisplay() {
        if (selectedPrerequisites.length === 0) {
            selectedPrerequisitesList.innerHTML = '<div class="text-muted">No prerequisites selected yet</div>';
            return;
        }

        selectedPrerequisitesList.innerHTML = selectedPrerequisites.map(course => `
                    <div class="prerequisite-tag">
                        ${course.name}
                        <button type="button" class="remove-btn" onclick="removeSelectedPrerequisite(${course.id})">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                `).join('');
    }

    function updateHiddenSkillsField() {
        const options = selectedSkillIds.querySelectorAll('option');
        options.forEach(option => {
            option.selected = false;
        });

        selectedSkills.forEach(skill => {
            const option = selectedSkillIds.querySelector(`option[value="${skill.id}"]`);
            if (option) {
                option.selected = true;
            }
        });
    }

    function updateHiddenPrerequisitesField() {
        // Clear existing options
        selectedPrerequisiteIds.innerHTML = '';

        // Add new options for selected prerequisites
        selectedPrerequisites.forEach(course => {
            const option = document.createElement('option');
            option.value = course.id;
            option.text = course.name;
            option.selected = true;
            selectedPrerequisiteIds.appendChild(option);
        });
    }

    // Close search results when clicking outside
    document.addEventListener('click', function (e) {
        if (!skillSearch.contains(e.target) && !searchResults.contains(e.target)) {
            searchResults.style.display = 'none';
        }
        if (!prerequisiteSearch.contains(e.target) && !prerequisiteSearchResults.contains(e.target)) {
            prerequisiteSearchResults.style.display = 'none';
        }
    });

    // Auto-disable price when free is checked
    const isFreeCheckbox = document.getElementById('isFreeCheckbox');
    const priceInput = document.getElementById('priceInput');

    if (isFreeCheckbox && priceInput) {
        isFreeCheckbox.addEventListener('change', function () {
            if (this.checked) {
                priceInput.value = '0';
                priceInput.disabled = true;
            } else {
                priceInput.disabled = false;
            }
        });

        if (isFreeCheckbox.checked) {
            priceInput.disabled = true;
        }
    }

    initializeSelectedSkills();
    window.removeSelectedSkill = removeSelectedSkill;
    window.removeSelectedPrerequisite = removeSelectedPrerequisite;
});