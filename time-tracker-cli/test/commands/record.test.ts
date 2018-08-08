import {expect, test} from '@oclif/test'

describe('record', () => {
  test
  .stdout()
  .command(['record'])
  .it('runs record', ctx => {
    expect(ctx.stdout).to.contain('record world')
  })

  test
  .stdout()
  .command(['record', '--name', 'jeff'])
  .it('runs record --name jeff', ctx => {
    expect(ctx.stdout).to.contain('record jeff')
  })
})