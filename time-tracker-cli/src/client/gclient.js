'use strict';

const {google} = require('googleapis');
const opn = require('opn');
const http = require('http');
const querystring = require('querystring');
const url = require('url');

class GClient {
  constructor(client_id, scope, redirect_uri_path) {
    this.client_id = client_id;
    this.scope = scope;
    this.redirect_uri_path = redirect_uri_path;
    this.redirect_uri = 'http://localhost:8080' + redirect_uri_path;
    this.oAuth2Client = new google.auth.OAuth2();
    this.codeVerifier = this.oAuth2Client.generateCodeVerifier().codeVerifier;
    this.querys = querystring.stringify({
      scope: this.scope,
      response_type: 'code',
      redirect_uri: this.redirect_uri,
      client_id: this.client_id,
      code_challenge: this.codeVerifier
    });
    this.authorize_url = 'https://accounts.google.com/o/oauth2/v2/auth?' + this.querys;
    console.log(this.authorize_url);
  }

  async authenticate(cb) {
    return new Promise((resolve, reject) => {
      const server = http.createServer(async (req, res) =>  {
        try {
          if (req.url.indexOf(this.redirect_uri_path) > -1) {
            const qs = querystring.parse(url.parse(req.url).query);
            res.end(
              'Authentication successful! Please return to console.'
            );
            server.close();
            resolve(cb({codeVerifier: this.codeVerifier, authorizationCode: qs.code}));
          }
        } catch(e) {
          reject(e);
        }
      }).listen(8080, () => {
        // open the browser to the authorize url to start the workflow
        opn(this.authorize_url, {wait: false}).then(cp => cp.unref());
      });
    });
  }
}

module.exports = GClient;
