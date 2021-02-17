<template>
  <div class="home">
    <div v-if="!$auth.loading">
      <div></div>
      <button v-if="!$auth.isAuthenticated" @click="login">Log in</button>

      <div
        class="container"
        v-if="$auth.isAuthenticated && showPage"
        style="padding-top: 30px"
      >
        <section class="hero">
          <div class="hero-body">
            <p class="title">Welcome</p>
            <!-- <p class="subtitle">{{ $auth.user.given_name }} {{ $auth.user.family_name }}</p> -->
            <p class="subtitle" v-if="orientation">{{ orientation.name }}</p>
          </div>
        </section>

        <section class="hero">
          <div class="hero-body">
            <div class="container">
              <section class="section">
                <div class="columns">
                  <div class="column is-8 is-offset-2">
                    <div class="content is-medium">
                      <h2 class="subtitle is-4">{{ currentDay }}</h2>
                      <h1 class="title">Getting Started</h1>
                      <p>
                        Congratulations on being part of the team! The whole
                        company welcomes you and we look forward to a successful
                        journey with you! Welcome aboard!
                      </p>
                      <p v-if="isDeckComplete">
                        Thank you for completing the prepared orientation by our team!
                      </p>
                    </div>
                  </div>
                </div>
              </section>
            </div>
          </div>
        </section>

        <div class="divider"></div>
        <section class="hero">
          <div class="hero-body">
            <ul
              class="steps is-narrow is-large is-centered has-content-centered"
              v-if="orientation"
            >
              <li
                class="steps-segment"
                v-for="record in orientation.categoryWithDeck"
                :key="record.name"
                :class="{ 'is-active has-gaps': record.isActive }"
              >
                <a href="javascript:void(0)" class="has-text-dark" @click="setCurrentDeck(record.name)">
                  <span class="steps-marker">
                    <span class="icon">
                      <i
                        :class="{
                          'fas fa-folder-open': record.isActive,
                          'fas fa-folder': !record.isActive,
                        }"
                      ></i>
                    </span>
                  </span>
                  <div class="steps-content">
                    <p class="heading">{{ record.name }}</p>
                  </div>
                </a>
              </li>
            </ul>
          </div>
        </section>

        <!-- Image -->
        <section class="hero" v-if="currentDeck">
          <div class="hero-body">
            <div class="container">
              <section class="section">
                <div class="columns">
                  <div class="column is-8 is-offset-2">
                    <div class="content is-medium">
                      <h1 class="title">{{ currentDeck.name }}</h1>
                      <p>
                        {{ currentDeck.description }}
                      </p>
                    </div>
                  </div>
                </div>
              </section>

              <div class="columns" v-if="currentDeckFiles">
                <div class="column is-8 is-offset-2">
                  <div
                    v-for="(file,index) in currentDeckFiles.files"
                    :key="file.FileName"

                  >

                    <pdf :src="file.dataURL" :page="pdfConfig[index].page"  @num-pages="getTotalNum"/>

                    <div>
                      Page {{ pdfConfig[index].page }} of {{ pdfConfig[index].totalPage }}
                    </div>
                    <div class="buttons">
                      <button class="button" @click="pdfConfig[index].page = pdfConfig[index].page- 1" v-if="pdfConfig[index].page>1">Previous Page</button>
                      <button class="button" @click="pdfConfig[index].page = pdfConfig[index].page+ 1" v-if="pdfConfig[index].page<pdfConfig[index].totalPage">Next Page</button>
                    </div>
                  </div>
                </div>
              </div>

              <div class="is-divider"></div>

              <div class="columns" v-if="!isDeckComplete">
                <div class="column is-8 is-offset-2">
                  <button
                    class="button bd-fat-button is-primary is-light bd-pagination-next"
                    href="javascript:void(0)"
                    @click="nextOrientation"
                    :disabled="!canContinute"
                  >
                    <span>
                      <strong v-if="!isLastDeck">Next Orientation File</strong>
                      <strong v-if="isLastDeck">Complete Orientation</strong>
                    </span>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios'
import moment from 'moment'
import pdf from 'vue-pdf'
export default {
  components: {
    pdf
  },
  data () {
    return {
      orientation: null,
      isActiveFound: false,
      currentDeck: null,
      currentDeckFiles: null,
      pdfConfig: [],
      pdfIndex: 0,
      showPage: false
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
    },
    currentDay () {
      return moment(new Date()).format('MMMM DD, YYYY')
    },
    email: function () {
      if (this.$auth.isAuthenticated) {
        return this.$auth.user.email
      }
      return ''
    },
    isLastDeck () {
      if (!this.orientation) {
        return false
      }
      const deckCount = this.orientation.categoryWithDeck.length
      const foundInActive = this.orientation.categoryWithDeck.findIndex(el => !el.isComplete)
      return foundInActive === (deckCount - 1)
    },
    canContinute () {
      const isCompleted = this.pdfConfig.filter(el => {
        return el.page >= el.totalPage
      })

      return isCompleted.length === this.pdfConfig.length
    },
    isDeckComplete () {
      if (!this.orientation) {
        return false
      }
      const incompleteDeck = this.orientation.categoryWithDeck.find(el => !el.isComplete)
      if (!incompleteDeck) {
        return true
      } else {
        return false
      }
    }
  },
  watch: {
    email: function (val) {
      if (val) {
        this.getOrientation()
      }
    }
  },
  methods: {
    setCurrentDeck (deckName) {
      if (!this.isDeckComplete) {
        alert('Please complete the orientation viewing before you can use this navigation.')
        return
      }
      this.currentDeck = this.orientation.categoryWithDeck.find(el => el.name === deckName)
      this.pdfIndex = 0
      this.pdfConfig = []
      this.getOrientationFiles()
    },
    getTotalNum (payload) {
      if (payload) {
        this.pdfConfig[this.pdfIndex].totalPage = payload
        this.pdfIndex += 1
      }
    },
    tagActive (isComplete) {
      if (!this.isActiveFound && !isComplete) {
        this.isActiveFound = true
        return 'is-active has-gaps'
      }
    },
    login () {
      this.$auth.loginWithRedirect()
    },
    logout () {
      this.$auth.logout({
        returnTo: window.location.origin
      })
    },
    async getOrientation () {
      this.$isLoading(true)
      this.currentDeck = null
      this.currentDeckFiles = []
      this.isActiveFound = false
      this.pdfIndex = 0
      this.pdfConfig = []
      const token = await this.$auth.getIdTokenClaims()
      const record = (
        await axios.get(
          `https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/getorientation/${this.email}`,
          {
            headers: {
              authorization: 'bearer ' + token.__raw
            }
          }
        )
      ).data

      if (!record) {
        this.$isLoading(false)
        return
      }
      record.categoryWithDeck.forEach((el, index) => {
        if (!this.isActiveFound && !el.isComplete) {
          this.isActiveFound = true
          el.isActive = true
          this.currentDeck = el
          this.getOrientationFiles()
        } else {
          el.isActive = false
        }
      })

      this.orientation = record
      this.$isLoading(false)
    },
    async getOrientationFiles () {
      this.$isLoading(true)
      const token = await this.$auth.getIdTokenClaims()
      const record = (
        await axios.get(
          `https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/getorientationfiles/${this.currentDeck.category}/${this.currentDeck.name}`,
          {
            headers: {
              authorization: 'bearer ' + token.__raw
            }
          }
        )
      ).data
      const numOfPdf = record.files.length
      for (let x = 0; x < numOfPdf; x++) {
        this.pdfConfig.push({
          page: 1,
          totalPage: 0
        })
      }
      this.currentDeckFiles = record
      this.$isLoading(false)
    },
    async nextOrientation () {
      const token = await this.$auth.getIdTokenClaims()

      await axios.post(
        'https://ninjamon-orientationapp.azurewebsites.net/api/mongodb/completeorientation',
        {
          deckName: this.currentDeck.name,
          email: this.$auth.user.email
        },
        {
          headers: {
            'Content-type': 'application/json',
            Authorization: 'bearer ' + token.__raw
          }
        }
      )
      this.getOrientation()
    }
  },
  mounted () {
    if (this.isAdmin) {
      this.$router.push('dashboard')
    } else {
      this.showPage = true
      this.getOrientation()
    }
  }
}
</script>
