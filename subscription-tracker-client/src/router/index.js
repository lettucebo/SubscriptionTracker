import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import Subscriptions from '../views/Subscriptions.vue'
import Report from '../views/Report.vue'
import SubscriptionForm from '../views/SubscriptionForm.vue'
import Calendar from '../views/Calendar.vue'

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
    path: '/report',
    name: 'Report',
    component: Report
  },
  {
    path: '/subscription-form/:id?',
    name: 'SubscriptionForm',
    component: SubscriptionForm
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
