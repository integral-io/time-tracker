import {expect, test} from '@oclif/test'

describe('config', () => {
  test
  .stdout()
  .command(['config'])
  .it('runs config', ctx => {
    expect(ctx.stdout).to.contain('hello world')
  })

  test
  .stdout()
  .command(['config', '--name', 'jeff'])
  .it('runs config --name jeff', ctx => {
    expect(ctx.stdout).to.contain('hello jeff')
  })
})