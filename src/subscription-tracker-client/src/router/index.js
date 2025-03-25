import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import Subscriptions from '../views/Subscriptions.vue'
import SubscriptionForm from '../views/SubscriptionForm.vue'
import Calendar from '../views/Calendar.vue'
import Categories from '../views/Categories.vue'

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/calendar',
    name: 'Calendar',
    component: Calendar
  },
  {
    path: '/subscriptions',
    name: 'Subscriptions',
    component: Subscriptions
  },
  {
    path: '/categories',
    name: 'Categories',
    component: Categories
  },
  {
    path: '/subscription-form',
    name: 'CreateSubscription',
    component: SubscriptionForm
  },
  {
    path: '/subscription-form/:id',
    name: 'EditSubscription',
    component: SubscriptionForm
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
