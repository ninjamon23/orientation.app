<template>
  <div>
    <!-- START NAV -->
    <nav class="navbar">
      <div class="container">
        <div class="navbar-brand">
          <router-link
            to="/"
            class="navbar-item is-size-5 has-text-weight-semibold"
          >
            <h1 class="is-size-5 has-text-weight-semibold">
              Orientation App
            </h1>
          </router-link>
          <a class="navbar-item" href="../">
          </a>
          <a
            class="navbar-burger burger"
            data-target="navbarMenu"
            @click="toggleMenu = !toggleMenu"
          >
            <span></span>
            <span></span>
            <span></span>
          </a>
        </div>
        <div
          id="navbarMenu"
          class="navbar-menu"
          :class="{ 'is-active': toggleMenu }"
        >
          <div class="navbar-end">
            <!-- <div class="navbar-item">
              <div class="control has-icons-left">
                <input
                  class="input is-rounded"
                  type="email"
                  placeholder="Search"
                />
                <span class="icon is-left">
                  <i class="fa fa-search"></i>
                </span>
              </div>
            </div> -->
            <router-link v-if="isAdmin"
                to="/hiredeck"
                class="navbar-item is-size-5 has-text-weight-semibold"
                >Orientation Deck</router-link
              >
              <router-link v-if="isAdmin"
                to="/newhires"
                class="navbar-item is-size-5 has-text-weight-semibold"
                >New Hires</router-link
              >
              <router-link v-if="isAdmin"
                to="/dashboard"
                class="navbar-item is-size-5 has-text-weight-semibold"
                >Dashboard</router-link
              >
          </div>
          <div
            class="navbar-item has-dropdown is-hoverable is-size-5 has-text-weight-semibold"
          >
            <a class="navbar-link">
              <!-- {{ $auth.user.given_name }} -->
              <i class="fas fa-user" style="margin-left: 10px"></i>
            </a>
            <div class="navbar-dropdown">
              <router-link
                to="/profile"
                class="navbar-item is-size-5 has-text-weight-semibold"
                >Profile</router-link
              >
              <hr class="navbar-divider" />
              <a
                class="navbar-item is-size-5 has-text-weight-semibold"
                @click="logout"
                >Logout</a
              >
            </div>
          </div>
        </div>
      </div>
    </nav>
    <!-- END NAV -->
    <router-view />
    <!-- <footer class="footer">
      <div class="content has-text-centered">
        <p>
          <strong>Training Module</strong> by
          <a href="#">Jordan Capoquian</a>
        </p>
      </div>
    </footer> -->
  </div>
</template>
<script>
export default {
  data () {
    return {
      toggleMenu: false
    }
  },
  computed: {
    isAdmin () {
      const temporaryAdmins = ['jordan.capoquian@gmail.com', 'chinky.bocar@planate.net']
      const result = temporaryAdmins.find(el => el === this.$auth.user.email)
      if (result) {
        return true
      } else {
        return false
      }
    }
  },
  methods: {
    logout () {
      this.$auth.logout({
        returnTo: window.location.origin
      })
    }
  }
}
</script>
<style>
nav.navbar {
  height: 6rem !important;
  box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06) !important;
}
</style>
