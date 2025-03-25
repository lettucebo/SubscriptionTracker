<template>
  <div class="categories">
    <h1>Categories Management</h1>
    <div class="actions">
      <button @click="showCreateForm = true" class="btn btn-primary">
        <i class="fas fa-plus me-2"></i>Add New Category
      </button>
    </div>

    <table class="table table-hover">
      <thead>
        <tr>
          <th><i class="fas fa-tag me-2"></i>Name</th>
          <th><i class="fas fa-info me-2"></i>Description</th>
          <th><i class="fas fa-palette me-2"></i>Color</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="category in categories" :key="category.id">
          <td>{{ category.name }}</td>
          <td>{{ category.description }}</td>
          <td>
            <div class="color-preview" :style="{'background-color': category.colorCode}"></div>
            {{ category.colorCode }}
          </td>
          <td>
            <button @click="editCategory(category)" class="btn btn-sm btn-warning me-2">
              <i class="fas fa-pen-to-square me-1"></i>Edit
            </button>
            <button @click="deleteCategory(category.id)" class="btn btn-sm btn-danger">
              <i class="fas fa-trash-can me-1"></i>Delete
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Create/Edit Modal -->
    <div v-if="showCreateForm || editingCategory" class="modal" style="display: block">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              <i class="fas" :class="editingCategory ? 'fa-pen me-2' : 'fa-folder me-2'"></i>
              {{ editingCategory ? 'Edit Category' : 'New Category' }}
            </h5>
            <button type="button" class="btn-close" aria-label="Close" @click="closeModal"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="submitForm">
              <div class="form-group">
                <label><i class="fas fa-tag me-2"></i>Name</label>
                <input v-model="formData.name" type="text" class="form-control" required>
              </div>
              <div class="form-group">
                <label><i class="fas fa-info me-2"></i>Description</label>
                <textarea v-model="formData.description" class="form-control"></textarea>
              </div>
              <div class="form-group">
                <label><i class="fas fa-palette me-2"></i>Color</label>
                <div class="color-picker-container">
                  <input 
                    v-model="formData.colorCode" 
                    type="color" 
                    class="form-control color-input"
                  >
                  <div class="color-suggestions">
                    <div 
                      v-for="color in suggestedColors" 
                      :key="color" 
                      class="color-suggestion" 
                      :style="{'background-color': color}"
                      @click="formData.colorCode = color"
                    ></div>
                  </div>
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" @click="closeModal">
                  <i class="fas fa-times me-2"></i>Cancel
                </button>
                <button type="submit" class="btn btn-primary">
                  <i class="fas" :class="editingCategory ? 'fa-check me-2' : 'fa-plus me-2'"></i>
                  {{ editingCategory ? 'Update' : 'Create' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { config } from '@/config';

export default {
  name: 'CategoriesManagement',
  data() {
    return {
      categories: [],
      showCreateForm: false,
      editingCategory: null,
      formData: {
        name: '',
        description: '',
        colorCode: '#3A86FF'
      },
      suggestedColors: [
        '#E63946', '#F28C28', '#FFBE0B', '#70B77E',
        '#00A8E8', '#3A86FF', '#6A4C93', '#FFB6B9',
        '#8ECAE6', '#95D5B2', '#F7D6E0', '#1D3557',
        '#2D3A3A', '#5E503F', '#6C757D'
      ]
    };
  },
  async created() {
    await this.fetchCategories();
  },
  methods: {
    async fetchCategories() {
      try {
        const response = await axios.get(`${config.baseUrl}/api/category`);
        this.categories = response.data;
      } catch (error) {
        console.error('Error fetching categories:', error);
      }
    },
    editCategory(category) {
      this.editingCategory = category;
      this.formData = { ...category };
    },
    async deleteCategory(id) {
      if (confirm('Are you sure you want to delete this category?')) {
        try {
          await axios.delete(`${config.baseUrl}/api/category/${id}`);
          await this.fetchCategories();
        } catch (error) {
          console.error('Error deleting category:', error);
        }
      }
    },
    async submitForm() {
      try {
        if (this.editingCategory) {
          await axios.put(`${config.baseUrl}/api/category/${this.editingCategory.id}`, this.formData);
        } else {
          await axios.post(`${config.baseUrl}/api/category`, this.formData);
        }
        await this.fetchCategories();
        this.closeModal();
      } catch (error) {
        console.error('Error saving category:', error);
      }
    },
    closeModal() {
      this.showCreateForm = false;
      this.editingCategory = null;
      this.formData = { 
        name: '', 
        description: '',
        colorCode: '#3A86FF'
      };
    }
  }
};
</script>

<style scoped>
.modal {
  background-color: rgba(0, 0, 0, 0.5);
}

.color-preview {
  width: 24px;
  height: 24px;
  border-radius: 4px;
  display: inline-block;
  margin-right: 8px;
  border: 1px solid rgba(0, 0, 0, 0.1);
}

.color-input {
  height: 40px;
  width: 100%;
  padding: 5px;
  cursor: pointer;
}

.color-picker-container {
  position: relative;
}

.color-suggestions {
  display: flex;
  flex-wrap: wrap;
  margin-top: 10px;
  gap: 8px;
}

.color-suggestion {
  width: 30px;
  height: 30px;
  border-radius: 4px;
  cursor: pointer;
  border: 1px solid rgba(0, 0, 0, 0.1);
  transition: transform 0.2s;
}

.color-suggestion:hover {
  transform: scale(1.1);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}
</style>
