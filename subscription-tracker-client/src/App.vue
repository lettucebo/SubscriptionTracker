<template>
  <div id="app">
    <nav class="navbar navbar-expand-lg navbar-light bg-light">
      <div class="container">
        <a class="navbar-brand" href="#">Subscription Tracker</a>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav me-auto">
            <li class="nav-item">
              <router-link to="/" class="nav-link">Home</router-link>
            </li>
            <li class="nav-item">
              <router-link to="/subscriptions" class="nav-link">Subscriptions</router-link>
            </li>
            <li class="nav-item">
              <router-link to="/calendar" class="nav-link">Calendar</router-link>
            </li>
            <li class="nav-item">
              <router-link to="/report" class="nav-link">Report</router-link>
            </li>
          </ul>
        </div>
      </div>
    </nav>
    <div class="container mt-3">
      <router-view />
    </div>
  </div>
</template>

<script>
export default {
  name: 'App'
}
</script>

<style>
@import url("https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css");
@import url("https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css");

body {
  margin: 0;
  padding: 0;
  font-family: "Helvetica Neue", Helvetica, Arial, sans-serif;
}
</style>
