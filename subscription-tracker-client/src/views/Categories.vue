<template>
  <div class="categories">
    <h1>Categories Management</h1>
    <div class="actions">
      <button @click="showCreateForm = true" class="btn btn-primary">
        Add New Category
      </button>
    </div>

    <!-- Category Table -->
    <table class="table table-hover">
      <thead>
        <tr>
          <th>Name</th>
          <th>Description</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="category in categories" :key="category.id">
          <td>{{ category.name }}</td>
          <td>{{ category.description }}</td>
          <td>
            <button @click="editCategory(category)" class="btn btn-sm btn-warning mr-2">
              Edit
            </button>
            <button @click="deleteCategory(category.id)" class="btn btn-sm btn-danger">
              Delete
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
              {{ editingCategory ? 'Edit Category' : 'New Category' }}
            </h5>
            <button type="button" class="btn-close" aria-label="Close" @click="closeModal"></button>
          </div>
          <div class="modal-body">
            <form @submit.prevent="submitForm">
              <div class="form-group">
                <label>Name</label>
                <input v-model="formData.name" type="text" class="form-control" required>
              </div>
              <div class="form-group">
                <label>Description</label>
                <textarea v-model="formData.description" class="form-control"></textarea>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-secondary" @click="closeModal">
                  Cancel
                </button>
                <button type="submit" class="btn btn-primary">
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
import { config } from '@/config'

export default {
  name: 'CategoriesManagement',
  data() {
    return {
      categories: [],
      showCreateForm: false,
      editingCategory: null,
      formData: {
        name: '',
        description: ''
      }
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
      this.formData = { name: '', description: '' };
    }
  }
};
</script>

<style scoped>
.modal {
  background-color: rgba(0, 0, 0, 0.5);
}
</style>
